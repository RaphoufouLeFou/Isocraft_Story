import pygame
from os.path import exists
from tkinter import *
from tkinter.filedialog import *
from pygame.locals import *
from threading import Thread

class Blocks:
    def __init__(self):
        self.tex = {} # 3 block sides
        self.zoom = {} # zoom at which the textures had been generated
        self.threads = [] # avoid launching multiple threads per texture
        self._tex = [] # base texture
        self.names = []

        # sorry gotta add those manually and in order for now
        self.names = ['sand_side', 'red_sand', 'sandstone_side', 'bedrock',
                      'cobblestone', 'desert_log', 'desert_leaves',
                      'boka brick', 'boka conquer', 'boka fear', 'boka boom',
                      'boka home', 'boka beast']

        for i, name in enumerate(self.names):
            # load texture for later
            n = '../Textures/%s.png' %name
            if not exists(n): raise Exception('%s does not exist' %n)
            self._tex.append(
                pygame.transform.scale(
                    pygame.image.load(n).convert_alpha(),
                    (32, 32)
                )
            )

            # edit the name in a C# way
            s = ''
            for j, c in enumerate(name.replace('side', '').replace('top', '')):
                if j == 0 or name[j-1] == '_': s += c.upper()
                elif c != '_': s += c
            self.names[i] = s

    def make_tex(self, block):
        # make 3 texture sides for this newly encountered block
        if block: tex = self._tex[block-1]
        else: r = g = b = a = 255

        zoom = struct.zoom

        x2, y0 = struct.get2d(1, 1, 0)
        x1, y1 = struct.get2d(1, 1, 1)
        x0, y2 = struct.get2d(0, 0, 1)

        dx0, dy0 = x1-x0, y2-y1
        dx1, dy1 = x1-x0, y1-y0
        dx2, dy2 = x2-x1, y2-y1
        front = pygame.Surface((dx0, dy0), SRCALPHA)
        top = pygame.Surface((x2-x0, dy1), SRCALPHA)
        right = pygame.Surface((dx2, y2-y0), SRCALPHA)

        for i, dx, dy in [(0, dx0, dy0), (1, dx1, dy1), (2, dx2, dy2)]:
            for x in range(dx):
                for y in range(dy):
                    if block: r, g, b, a = tex.get_at((int(x*32/dx), int(y*32/dy)))
                    if i == 0: front.set_at((x, y), (r*0.7, g*0.7, b*0.7, a))
                    elif i == 1: top.set_at((x + int((1 - y/dy)*(x2-x1)), y), (r, g, b, a))
                    else: right.set_at((x, y + int((1 - x/dx)*(y1-y0))), (r*0.9, g*0.9, b*0.9, a))

        self.zoom[block] = zoom
        self.tex[block] = (front, top, right)
        self.threads.remove(block)

    def get_tex(self, block):
        if block not in self.tex or self.zoom[block] != struct.zoom:
            if block:
                if block and block not in self.threads: # make texture in thread
                    self.threads.append(block)
                    Thread(target=self.make_tex, args=(block,)).start()

                # get or make air texture while calculating if no other texture is available
                if block not in self.tex: return self.get_tex(0)
            else:
                self.threads.append(0)
                self.make_tex(0)

        return self.tex[block]

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
    def __init__(self, x, text, i, min, max, neg=False):
        self.x, self.i = x, i
        self.min, self.max = min, max
        self.neg = neg

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
                self.active = (self.x <= event.pos[0] < self.x+62
                               and event.pos[1] < 40)

                if self.active: self.col = BLACK
                else: self.refresh_size() # also reset color

            if event.type == KEYDOWN and self.active:
                if event.unicode.isnumeric() and self.n < self.max/10:
                    self.n = self.n*10 + int(event.unicode)
                    self.refreshed = False
                elif event.key == K_MINUS or event.key == K_KP_MINUS:
                    if self.neg:
                        self.n *= -1
                        self.refreshed = False
                elif event.key == K_BACKSPACE:
                    self.n = int(self.n/10)
                    self.refreshed = False
                elif event.key == K_RETURN or event.key == K_KP_ENTER:
                    self.refresh_size()

        # display
        screen.blit(self.text, (self.x, 12))
        pygame.draw.rect(screen, WHITE, Rect(self.x + 30, 12, 30, 16))
        text = font.render(str(self.n) + '_'*self.active, 1, self.col)
        screen.blit(text, (self.x + 30, 12))

    def refresh_ip(self):
        if self.i == -1: return # not concerned

        # refresh the input's value
        if self.i < 0: self.n = struct.origin[-2-self.i] # origin input
        else: self.n = struct.size[self.i] # size input

    def refresh_size(self):
        global blocks

        self.n = min(max(self.n, self.min), self.max)

        self.active = False
        self.col = DGRAY
        if self.refreshed: return # just unfocusing
        self.refreshed = True

        if self.i == -1: return
        if self.i >= 0:
            # resize map according to the input's value
            struct.size[self.i] = self.n
            struct.init_data()
        else:
            # change origin according to the input's value
            struct.origin[-2-self.i] = self.n

class Ui:
    def __init__(self):
        self.buttons = (Button(0, 'Load file', struct.load),
                        Button(102, 'Save', struct.save))
        self.inputs = (Input(254, ' x=', 0, 1, 100),
                       Input(316, ' y=', 1, 1, 100),
                       Input(378, ' z=', 2, 1, 100),
                       Input(440, 'ID:', -1, 0, len(blocks.names)),
                       Input(702, 'x0=', -2, 0, 100),
                       Input(764, 'dy=', -3, -16, 100, True),
                       Input(826, 'z0=', -4, 0, 100))

        self.back = pygame.Surface((900, 40))
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
        if self.inputs[3].n > len(blocks.names): name = '[unknown]'
        elif self.inputs[3].n: name = '(%s)' %blocks.names[self.inputs[3].n-1]
        else: name = '[Air]'
        screen.blit(font.render(name, 1, BLACK), (512, 12))

class Structure:
    # right, top, front
    face_indices = [(5, 7, 6, 4), (3, 2, 6, 7), (1, 3, 7, 5)]
    txt = [('IsoCraft Story schematics', '.txt')]

    def __init__(self):
        self.size = [5, 5, 5]
        self.init_data()

    def init_data(self, origin=[0, 1, 0]):
        self.data = [[[-1]*self.size[2]
                      for _ in range(self.size[1])]
                      for _ in range(self.size[0])]
        self.origin = origin
        self.zoom = 1
        self.layer = 0

    def load(self):
        tk = mkwin()
        name = askopenfilename(filetypes=self.txt)
        if name:
            with open(name) as f:
                lines = f.read().split('\n')

            X, Y, Z = lines[1].split('.')
            origin = [int(X), int(Y), int(Z)]
            X, Y, Z = lines[0].split('.')
            self.size = X, Y, Z = [int(X), int(Y), int(Z)]
            data = [-1 if x == '' else int(x) for x in lines[2].split('.')]

            # put data into 3D array
            self.init_data(origin)
            x = y = z = 0
            for d in data:
                if z == Z: y, z = y+1, 0
                if y == Y: x, y = x+1, 0
                self.data[x][y][z] = d
                z += 1

            # refresh inputs
            for i in ui.inputs:
                i.refresh_ip()

            # spawn on top of loaded structure
            self.layer = self.size[1]-1
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
                        data += '.'
                        b = self.data[x][y][z]
                        if b != -1: data += str(b)

            with open(name, 'w') as f:
                f.write('%d.%d.%d\n%d.%d.%d\n%s' %(
                    *self.size, *self.origin, data[1:]))
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
                # find the 3d position of clicked point
                x = y = 0
                _x, _y = event.pos
                ok = 0
                while not ok:
                    if y < 0 or y >= self.size[2]:
                        ok = False
                        break
                    x0 = self.get2d(x, self.layer+1, y)[0]
                    x1, y0 = self.get2d(x+1, self.layer+1, y)
                    x02, y1 = self.get2d(x, self.layer+1, y+1)
                    # update boundaries since the collision box is not a rectangle
                    dx, t = x02-x0, (_y-y0)/(y1-y0)
                    x0, x1 = x0 + dx*t, x1 + dx*t
                    
                    ok = 3 # both X and Z conditions are needed, use bitwise operations
                    if _y < y0: y -= 1
                    elif _y > y1: y += 1
                    else:
                        ok -= 2
                        
                        if _x < x0: x -= 1
                        elif _x > x1: x += 1
                        else: ok -= 1

                        if x < 0 or x >= self.size[0]:
                            ok = False
                            break # x boundaries change as Z changes, only check after Z is ok
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
        m = 1/self.size[2] if self.size[0] < self.size[2] else 1/self.size[0]
        return int(20 + 710*x*m + 150*(1-z*m)), \
               int(60 + 420*z*m + 400*m*(self.layer-y+1)*self.zoom)

    def draw_cube(self, x, y, z, block, alpha):
        x2, y0 = self.get2d(x+1, y+1, z)
        x1, y1 = self.get2d(x+1, y+1, z+1)
        x0, y2 = self.get2d(x, y, z+1)
        a, b, c = blocks.get_tex(block)
        surf = pygame.Surface((x2-x0, y2-y0), SRCALPHA)
        surf.blit(a, (0, y1-y0))
        surf.blit(b, (1, 0))
        surf.blit(c, (x1-x0, 0))
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

                        self.draw_cube(x, y, z, b, alpha)

        # selection grid
        y = self.layer+1
        for i in range(self.size[0]+1):
            a, b = self.get2d(i, y, 0), self.get2d(i, y, self.size[2])
            pygame.draw.line(screen, GRID2, a, b)
        for i in range(self.size[2]+1):
            a, b = self.get2d(0, y, i), self.get2d(self.size[0], y, i)
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

BLACK, DGRAY, LGRAY, WHITE, CYAN, GRID1, GRID2 = \
                             (0,   0,   0  ), (100, 100, 100), \
                             (200, 200, 200), (255, 255, 255), \
                             (200, 230, 255), \
                             (120, 150, 180), (50, 50, 50)

pygame.init()
pygame.display.set_caption('Schematic editor')
screen = pygame.display.set_mode((900, 550))
font = pygame.font.SysFont('consolas', 16)
clock = pygame.time.Clock()

blocks = Blocks()
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
