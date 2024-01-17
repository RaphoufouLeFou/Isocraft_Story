import pygame
from tkinter import *
from tkinter.filedialog import askopenfilename, asksaveasfilename
from pygame.locals import *

# TODO: set offset
# TODO: change size

class Button:
    def __init__(self, x, text, f):
        self.x, self.f = x, f
        self.text = font.render(text, 1, BLACK)
        self.textw = self.text.get_width() >> 1

    def update(self, event):
        # click
        if event is not None and self.x <= event.pos[0] < self.x+100 and event.pos[1] < 40:
            self.f()

        # display
        pygame.draw.rect(screen, WHITE, Rect(self.x, 0, 100, 40))
        screen.blit(self.text, (self.x + 50 - self.textw, 12))

class Input:
    def __init__(self, x, text, i):
        self.x, self.i = x, i
        self.text = font.render(text, 1, BLACK)
        self.n = 0
        self.active = False

    def update(self, events):
        # edit content
        for event in events:
            if event.type == MOUSEBUTTONDOWN:
                self.active = self.x <= event.pos[0] < self.x+100 and event.pos[1] < 40

            if event.type == KEYDOWN and self.active:
                if event.unicode.isnumeric() and self.n < 100: self.n = self.n*10 + int(event.unicode)
                elif event.key == K_BACKSPACE: self.n = int(self.n/10)

        # display
        screen.blit(self.text, (self.x, 12))
        pygame.draw.rect(screen, WHITE, Rect(self.x + 20, 12, 30, 16))
        screen.blit(font.render(str(self.n) + '_'*self.active, 1, BLACK if self.active else DGRAY), (self.x + 20, 12))

class Ui:
    def __init__(self):
        self.buttons = (Button(0, 'Load file', load_file), Button(102, 'Save', save_file))
        self.inputs = (Input(254, 'x=', 0), Input(306, 'y=', 1), Input(358, 'z=', 2))
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

txt = [('isocraft schematics', '.txt')]
def load_file():
    global size, data
    tk = mkwin()
    name = askopenfilename(filetypes=txt)
    if name:
        with open(name) as f:
            lines = f.read().split('\n')
        x, y, z = lines[0].split('.')
        size = x, y, z = int(x), int(y), int(z)
        data = [-1 if x == '' else int(x) for x in lines[2].split('.')]
    tk.destroy()

def save_file():
    tk = mkwin()
    name = asksaveasfilename(filetypes=txt)
    if name:
        if not name.endswith('.txt'): name += '.txt'
        with open(name, 'w') as f:
            f.write('%d.%d.%d\n0.0.0\n%s' %(*size, '.'.join(str(d) for d in data)))
    tk.destroy()

def display():
    pass

blocks = []
blocks.insert(0, None)
size = [5, 5, 5]
data = [0]*5*5*5

BLACK, DGRAY, LGRAY, WHITE, CYAN = (0, 0, 0), (100, 100, 100), (200, 200, 200), (255, 255, 255), (230, 250, 255)

pygame.init()
pygame.display.set_caption('Schematic editor')
screen = pygame.display.set_mode((640, 480))
font = pygame.font.SysFont('consolas', 16)
clock = pygame.time.Clock()

ui = Ui()

running = True
while running:
    events = pygame.event.get()
    for event in events:
        if event.type == QUIT:
            pygame.quit()
            running = False

    screen.fill(CYAN)

    ui.update(events)
    display()
    pygame.display.flip()
    clock.tick(60)

quit()
