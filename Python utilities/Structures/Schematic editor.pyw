import pygame
from tkinter import *
from tkinter.filedialog import askopenfilename, asksaveasfilename
from pygame.locals import *

# TODO: set offset
# TODO: change size

def mkwin():
    tk = Tk()
    tk.wm_attributes('-alpha', 0)
    return tk

txt = [('isocraft schematics', '.txt')]
def load_file():
    global size, data
    tk = mkwin()
    name = askopenfilename(filetypes=txt)
    print(name, name is None)
    with open(name) as f:
        lines = f.read().split('\n')
    x, y, z = lines[0].split('.')
    size = x, y, z = int(x), int(y), int(z)
    data = [int(x) for x in lines[2].split('.')]
    tk.destroy()

def save_file():
    tk = mkwin()
    name = asksaveasfilename(filetypes=txt)
    with open(name, 'w') as f:
        f.write('%d.%d.%d\n0.0.0\n%s' %(*size, '.'.join(data)))
    tk.destroy()

def ui(events):
    pass

def display():
    pass

blocks = []
blocks.insert(0, None)
size = [5, 5, 5]
data = [0]*5*5*5

pygame.init()
pygame.display.set_caption('Schematic editor')
screen = pygame.display.set_mode((640, 480))
font = pygame.font.SysFont('consolas', 16)
clock = pygame.time.Clock()

running = True
while running:
    events = pygame.event.get()
    for event in events:
        if event.type == QUIT:
            pygame.quit()
            running = False


    screen.fill((255, 255, 255))

    ui(events)
    display()
    pygame.display.flip()
    clock.tick(60)

quit()
