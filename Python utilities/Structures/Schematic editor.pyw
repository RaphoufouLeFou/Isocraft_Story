import pygame
from os.path import exists
from tkinter import *
from tkinter.filedialog import askopenfilename, asksaveasfilename
from pygame.locals import *

# TODO: set offset
# TODO: keep blocks when resizing

class Button:
    def __init__(self, x, text, f):
        self.x, self.f = x, f
        self.text = font.render(text, 1, BLACK)
        self.textw = self.text.get_width() >> 1

    def update(self, event):
        # click
        if event is not None \
        and self.x <= event.pos[0] < self.x+100 \
        and event.pos[1] < 40:
            self.f()

        # display
        pygame.draw.rect(screen, WHITE, Rect(self.x, 0, 100, 40))
        screen.blit(self.text, (self.x + 50 - self.textw, 12))

class Input:
    def __init__(self, x, text, i, min, max):
        self.x, self.i = x, i
        self.min, self.max = min, max

        self.n = 0
        self.active = False
        self.refreshed = True

        self.text = font.render(text, 1, BLACK)
        self.col = DGRAY
        self.refresh_ip()

    def update(self, events):
        # edit content
        for event in events:
            if event.type == MOUSEBUTTONDOWN:
                self.active = (self.x <= event.pos[0] < self.x+52
                               and event.pos[1] < 40)

                if self.active: self.col = BLACK
                else: self.refresh_size() # also reset color

            if event.type == KEYDOWN and self.active:
                if event.unicode.isnumeric():
                    self.n = self.n*10 + int(event.unicode)
                    self.refreshed = False
                elif event.key == K_BACKSPACE:
                    self.n = int(self.n/10)
                    self.refreshed = False
                elif event.key == K_RETURN or event.key == K_KP_ENTER:
                    self.refresh_size()

        # display
        screen.blit(self.text, (self.x, 12))
        pygame.draw.rect(screen, WHITE, Rect(self.x + 20, 12, 30, 16))
        text = font.render(str(self.n) + '_'*self.active, 1, self.col)
        screen.blit(text, (self.x + 20, 12))

    def refresh_ip(self):
        if self.i == -1: return # not a size input

        # refresh the input's value according to size
        self.n = struct.size[self.i]

    def refresh_size(self):
        self.n = min(max(self.n, self.min), self.max)

        if self.i == -1: return

        # resize map according to the input's value
        global blocks

        if self.n < 1: return

        if self.refreshed: return
        self.active = False
        self.col = DGRAY
        self.refreshed = True

        struct.size[self.i] = self.n
        struct.init_data()

class Ui:
    def __init__(self):
        self.buttons = (Button(0, 'Load file', struct.load),
                        Button(102, 'Save', struct.save))
        self.inputs = (Input(254, 'x=', 0, 1, 100),
                       Input(306, 'y=', 1, 1, 100),
                       Input(358, 'z=', 2, 1, 100),
                       Input(410, 'ID', -1, 0, len(colors)))

        self.back = pygame.Surface((640, 40))
        self.back.fill(LGRAY)
        self.back.blit(font.render('Size:', 1, BLACK), (204, 12))

    def update(self, events):
        screen.blit(self.back, (0, 0))

        # get event to optimize buttons update
        click = None
        for event in events:
            if event.type == MOUSEBUTTONDOWN:
                click = event
                break

        # update and display widgets
        for b in self.buttons:
            b.update(click)
        for i in self.inputs:
            i.update(events)

        # show current block name
        if self.inputs[3].n: name = blocks_names[self.inputs[3].n-1]
        else: name = 'Air'
        screen.blit(font.render(name, 1, BLACK), (462, 12))

class Structure:
    # right, top, front
    face_indices = [(5, 7, 6, 4), (3, 2, 6, 7), (1, 3, 7, 5)]
    txt = [('IsoCraft Story schematics', '.txt')]

    def __init__(self, size=[5, 5, 5]):
        self.zoom = 1
        self.size = size
        self.init_data()

    def init_data(self):
        self.data = [[[-1]*self.size[2]
                      for _ in range(self.size[1])]
                      for _ in range(self.size[0])]
        self.layer = 0

    def load(self):
        tk = mkwin()
        name = askopenfilename(filetypes=self.txt)
        if name:
            with open(name) as f:
                lines = f.read().split('\n')

            X, Y, Z = lines[0].split('.')
            self.size = X, Y, Z = [int(X), int(Y), int(Z)]
            data = [-1 if x == '' else int(x) for x in lines[2].split('.')]

            # put data into 3D array
            self.init_data()
            x = y = z = 0
            for d in data:
                if z == Z: x, z = x+1, 0
                if x == X: y, x = y+1, 0
                self.data[x][y][z] = d
                z += 1

            # refresh inputs
            for i in ui.inputs:
                i.refresh_ip()
        tk.destroy()

    def save(self):
        tk = mkwin()
        name = asksaveasfilename(filetypes=self.txt)
        if name:
            if not name.endswith('.txt'): name += '.txt'
            data = ''
            for x in range(self.size[0]):
                for y in range(self.size[1]):
                    for z in range(self.size[2]):
                        if data: data += '.'
                        b = self.data[x][y][z]
                        if b != -1: data += str(b)

            with open(name, 'w') as f:
                f.write('%d.%d.%d\n0.0.0\n%s' %(*self.size, data))
        tk.destroy()

    def update(self, events):
        pressed = pygame.key.get_pressed()
        zoom = pressed[K_LCTRL] or pressed[K_RCTRL]
        for event in events:
            if event.type == MOUSEWHEEL:
                if zoom:
                    if event.y > 0: self.zoom *= 1.2
                    else: self.zoom /= 1.2
                else:
                    self.layer = min(max(self.layer+event.y, 0), self.size[1]-1)
            elif event.type == MOUSEBUTTONDOWN:
                x = y = 0
                _x, _y = event.pos
                ok = 0
                while not ok:
                    if x < 0 or y < 0 or x == self.size[0] or y == self.size[2]:
                        ok = False
                        break
                    x0, y0 = self.get2d(x, self.layer+1, y)
                    x1 = self.get2d(x+1, self.layer+1, y)[0]
                    y1 = self.get2d(x+1, self.layer+1, y+1)[1]
                    ok = 3
                    if _x < x0: x -= 1
                    elif _x > x1: x += 1
                    else: ok -= 1
                    if _y < y0: y -= 1
                    elif _y > y1: y += 1
                    else: ok -= 2
                    ok = not ok
                if ok:
                    if event.button == 1:
                        self.data[x][self.layer][y] = ui.inputs[3].n
                    elif event.button == 2:
                        ui.inputs[3].n = self.data[x][self.layer][y]
                    elif event.button == 3:
                        self.data[x][self.layer][y] = -1

    def get2d(self, x, y, z):
        x, z = x*self.zoom, z*self.zoom
        return int(20 + 500*x/self.size[0] + 100*(1-z/self.size[2])), \
               int(60 + 400*z/self.size[2] +
                   300/self.size[0]*(self.layer-y+1)*self.zoom)

    def draw_cube(self, x, y, z, col, alpha):
        R, G, B = col

        p = [self.get2d(_x, _y, _z) for _x in (x, x+1)
             for _y in (y, y+1) for _z in (z, z+1)]
        x0, y0 = p[1][0], p[2][1]
        p = [(p[0]-x0, p[1]-y0) for p in p]

        surf = pygame.Surface((p[4][0], p[1][1]), SRCALPHA)
        for i, indices in enumerate(self.face_indices):
            a, b, c, d = indices
            if (i == 0 and x < self.size[0]-1 and self.data[x+1][y][z] != -1) \
                or (i == 1 and y < self.size[1]-1
                    and self.data[x][y+1][z] != -1 and y != self.layer) \
                or (i == 2 and z < self.size[2]-1
                    and self.data[x][y][z+1] != -1):
                continue
            m = (0.7, 1, 0.9)[i]
            col = int(R*m), int(G*m), int(B*m)
            pygame.draw.polygon(surf, col, (p[a], p[b], p[c], p[d]))

        surf.set_alpha(alpha)
        screen.blit(surf, (x0, y0))

    def display(self):
        # back part of outlines
        p = [self.get2d(x, y, z) for x in (0, self.size[0])
                                 for y in (0, self.size[1])
                                 for z in (0, self.size[2])]
        for i in range(4):
            if not i&1: pygame.draw.line(screen, GRID1, p[i], p[i+1])
            if i < 6 and not i&2: pygame.draw.line(screen, GRID1, p[i], p[i+2])
            if i < 4: pygame.draw.line(screen, GRID1, p[i], p[i+4])

        # blocks
        for x in range(self.size[0]):
            for y in range(self.size[1]):
                for z in range(self.size[2]):
                    b = self.data[x][y][z]
                    if b != -1:
                        if y == self.layer: alpha = 255
                        elif y < self.layer: alpha = 230
                        else: alpha = 30

                        if b == 0: col, alpha = WHITE, alpha*0.3
                        else: col = colors[b-1]
                        self.draw_cube(x, y, z, col, alpha)

        # selection grid
        y = self.layer+1
        for i in range(self.size[0]+1 << 1):
            i, j = i>>1, i&1
            if j: a, b = self.get2d(i, y, 0), self.get2d(i, y, self.size[2])
            else: a, b = self.get2d(0, y, i), self.get2d(self.size[0], y, i)
            pygame.draw.line(screen, GRID2, a, b)

        # front part of outlines
        for i in range(4, 7):
            if not i&1: pygame.draw.line(screen, GRID1, p[i], p[i+1])
            if i < 6 and not i&2: pygame.draw.line(screen, GRID1, p[i], p[i+2])
            if i < 4: pygame.draw.line(screen, GRID1, p[i], p[i+4])

def mkwin():
    tk = Tk()
    tk.wm_attributes('-alpha', 0)
    return tk

def load_textures():
    # sorry gotta add them manually
    global blocks_names, colors
    blocks_names = ['sand_side', 'red_sand', 'sandstone_side', 'bedrock',
                    'cobblestone', 'oak_log', 'oak_leaves']
    colors = []

    for i, name in enumerate(blocks_names):
        n = '../Textures/%s.png' %name
        if not exists(n): n = n[:-3]+'jpg' # WHYYYYY
        surf = pygame.image.load(n).convert_alpha()
        surf = pygame.transform.scale(surf, (32, 32)) # WHYYYYY again
        r = g = b = 0
        for x in range(32):
            for y in range(32):
                _r, _g, _b, a = surf.get_at((x, y))
                a /= 255
                r, g, b = r + _r*a, g + _g*a, b + _b*a
        colors.append((r/1024, g/1024, b/1024))

        # also edit the name in a C# way
        s = ''
        for j, c in enumerate(name.lower().replace('side', '')):
            if j == 0 or name[j-1] == '_': s += c.upper()
            elif c != '_': s += c
        blocks_names[i] = s

BLACK, DGRAY, LGRAY, WHITE, CYAN, GRID1, GRID2 = \
                             (0,   0,   0  ), (100, 100, 100), \
                             (200, 200, 200), (255, 255, 255), \
                             (200, 230, 255), \
                             (120, 150, 180), (50, 50, 50)

pygame.init()
pygame.display.set_caption('Schematic editor')
screen = pygame.display.set_mode((640, 480))
font = pygame.font.SysFont('consolas', 16)
clock = pygame.time.Clock()

load_textures()
struct = Structure()
ui = Ui()

running = True
while True:
    events = pygame.event.get()
    for event in events:
        if event.type == QUIT:
            pygame.quit()
            running = False
    if not running: break

    screen.fill(CYAN)

    struct.update(events)
    struct.display()
    ui.update(events)

    pygame.display.flip()
    clock.tick(60)

quit()
