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
    def __init__(self, size=[5, 5, 5]):
        self.size = size
        self.init_data()

        self.txt = [('IsoCraft Story schematics', '.txt')]

    def init_data(self):
        from random import randint
        self.data = [[[0]*self.size[2]
                      for _ in range(self.size[1])]
                      for _ in range(self.size[0])]
        for y in range(self.size[1]):
            for x in range(self.size[0]):
                for z in range(self.size[2]):
                    if y == 0 or self.data[x][y-1][z] and randint(0, 3):
                        self.data[x][y][z] = 1
        self.layer = self.size[1]-1

    def load():
        tk = mkwin()
        name = askopenfilename(filetypes=txt)
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
                z += 1
                if z == Z: y, z = y+1, 0
                if y == Y: x, y = x+1, 0
                self.data[x][y][z] = d

            # refresh inputs
            for i in ui.inputs:
                ui.refresh_ip()
        tk.destroy()

    def save():
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
                self.layer = min(max(self.layer+event.y, 1), self.size[1]-1)

    def get2d(self, x, y, z):
        return int(20 + 500*x/self.size[0] + 100*(1-z/self.size[2])), \
               int(60 + 400*z/self.size[2] + 300/self.size[0]*(self.layer-y))

    def colorize(self, col, i):
        r, g, b = col
        m = (0.7, 1, 0.9)[i]
        return int(r*m), int(g*m), int(b*m)

    # right, top, front
    face_indices = [(5, 4, 6, 7), (3, 7, 6, 2), (1, 5, 7, 3)]
    def display(self):
        # outlines
        p = [self.get2d(x, y, z) for x in (0, self.size[0])
                                 for y in (0, self.size[1])
                                 for z in (0, self.size[2])]
        for i in range(7):
            if not i&1: pygame.draw.line(screen, DGRAY, p[i], p[i+1])
            if i < 6 and not i&2: pygame.draw.line(screen, DGRAY, p[i], p[i+2])
            if i < 4: pygame.draw.line(screen, DGRAY, p[i], p[i+4])

        # blocks
        for x in range(self.size[0]):
            for y in range(self.layer):
                for z in range(self.size[2]):
                    if self.data[x][y][z]:
                        p = [self.get2d(_x, _y, _z) for _x in (x, x+1)
                             for _y in (y, y+1) for _z in (z, z+1)]
                        i = 0
                        for a, b, c, d in self.face_indices:
                            pygame.draw.polygon(screen, self.colorize(DGRAY, i),
                                                (p[a], p[b], p[c], p[d]))
                            i += 1

        # selection grid
        #for x in range(self.size[0]):
        #    pygame.draw.line(screen, LGRAY, 

BLACK, DGRAY, LGRAY, WHITE, CYAN = (0,   0,   0  ), \
                                   (100, 100, 100), (200, 200, 200), \
                                   (255, 255, 255), (230, 250, 255)

pygame.init()
pygame.display.set_caption('Schematic editor')
screen = pygame.display.set_mode((640, 480))
font = pygame.font.SysFont('consolas', 16)
clock = pygame.time.Clock()

struct = Structure()
ui = Ui()

running = True
while running:
    events = pygame.event.get()
    for event in events:
        if event.type == QUIT:
            pygame.quit()
            running = False

    screen.fill(CYAN)

    struct.update(events)
    struct.display()
    ui.update(events)

    pygame.display.flip()
    clock.tick(60)

quit()
