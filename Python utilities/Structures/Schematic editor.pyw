import pygame
from tkinter import *
from tkinter.filedialog import askopenfilename, asksaveasfilename
from pygame.locals import *

# TODO: set offset
# TODO: keep blocks when resizing
# TODO: transparent air

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
                if event.unicode.isnumeric() and self.n < self.max/10:
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
        if self.n < self.min: self.n = self.min

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
                       Input(410, 'ID', -1, 0, 9))

        self.back = pygame.Surface((640, 40))
        self.back.fill(LGRAY)
        self.back.blit(font.render('Size:', 1, BLACK), (204, 12))

    def update(self, events):
        screen.blit(self.back, (0, 0))
        click = None
        for event in events:
            if event.type == MOUSEBUTTONDOWN:
                click = event
                break
        for b in self.buttons:
            b.update(click)
        for i in self.inputs:
            i.update(events)

def mkwin():
    tk = Tk()
    tk.wm_attributes('-alpha', 0)
    return tk

class Structure:
    # right, top, front
    face_indices = [(5, 7, 6, 4), (3, 2, 6, 7), (1, 3, 7, 5)]
    txt = [('IsoCraft Story schematics', '.txt')]

    def __init__(self, size=[5, 5, 5]):
        self.size = size
        self.init_data()

    def init_data(self):
        self.data = [[[-1]*self.size[2]
                      for _ in range(self.size[1])]
                      for _ in range(self.size[0])]
        self.layer = self.size[1]-1

    def load(self):
        tk = mkwin()
        name = askopenfilename(filetypes=self.txt)
        if name:
            with open(name) as f:
                lines = f.read().split('\n')

            X, Y, Z = lines[0].split('.')
            self.size = X, Y, Z = int(X), int(Y), int(Z)
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
        name = asksaveasfilename(filetypes=txt)
        if name:
            if not name.endswith('.txt'): name += '.txt'
            data = ''
            for x in range(self.size[0]):
                for y in range(self.size[1]):
                    for z in range(self.size[2]):
                        if data: data += '.'
                        data += str(self.data[x][y][z])

            with open(name, 'w') as f:
                f.write('%d.%d.%d\n0.0.0\n%s' %(*size, data))
        tk.destroy()

    def update(self, events):
        for event in events:
            if event.type == MOUSEWHEEL:
                self.layer = min(max(self.layer+event.y, 0), self.size[1]-1)

    def get2d(self, x, y, z):
        return int(20 + 500*x/self.size[0] + 100*(1-z/self.size[2])), \
               int(60 + 400*z/self.size[2] + 300/self.size[0]*(self.layer-y+1))

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
        for i in range(3):
            if not i&1: pygame.draw.line(screen, GRID1, p[i], p[i+1])
            if i < 6 and not i&2: pygame.draw.line(screen, GRID1, p[i], p[i+2])
            if i < 4: pygame.draw.line(screen, GRID1, p[i], p[i+4])

        # blocks
        for x in range(self.size[0]):
            for y in range(self.size[1]):
                for z in range(self.size[2]):
                    if self.data[x][y][z] != -1:
                        if y == self.layer: alpha = 255
                        elif y < self.layer: alpha = 230
                        else: alpha = 30
                        self.draw_cube(x, y, z, DGRAY, alpha)

        # selection grid
        y = self.layer+1
        for i in range(self.size[0]+1 << 1):
            i, j = i>>1, i&1
            if j: a, b = self.get2d(i, y, 0), self.get2d(i, y, self.size[2])
            else: a, b = self.get2d(0, y, i), self.get2d(self.size[0], y, i)
            pygame.draw.line(screen, GRID2, a, b)

        # front part of outlines
        for i in range(3, 7):
            if not i&1: pygame.draw.line(screen, GRID1, p[i], p[i+1])
            if i < 6 and not i&2: pygame.draw.line(screen, GRID1, p[i], p[i+2])
            if i < 4: pygame.draw.line(screen, GRID1, p[i], p[i+4])

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
