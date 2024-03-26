from math import ceil, sqrt
from os import listdir
from os.path import splitext
import pygame
from pygame.locals import SRCALPHA, Rect

def format_name(src, ext):
    if ext != '.png':
        print('Wrong image extension (%s), skipped %s' %(ext, src))

    dst = ''
    for i, c in enumerate(src):
        if not i or src[i-1] in '_ ': dst += c.upper()
        elif c not in '_ ': dst += c
    return dst

RES = 32 # texture resolution
TEXMAP = 'texmap.png'

images = {format_name(*splitext(path)):
          pygame.transform.scale(pygame.image.load(path), (RES, RES))
          for path in listdir()
          if '.py' not in path and '.meta' not in path and path != TEXMAP}

# get texmap size
l = len(images)
w = ceil(sqrt(l))
h = ceil(l/w)
texmap = pygame.Surface((w*RES, h*RES), SRCALPHA)

# blit images
keys = list(images)
r = RES/2
black, purple = (0, 0, 0), (255, 0, 255)
for i in range(w*h):
    x, y = i%w*RES, i//w*RES
    if i < len(keys): texmap.blit(images[keys[i]], (x, y))
    else: # black and purple fill
        pygame.draw.rect(texmap, black, Rect(x, y, RES, RES))
        pygame.draw.rect(texmap, purple, Rect(x, y, r, r))
        pygame.draw.rect(texmap, purple, Rect(x+r, y+r, r, r))

pygame.image.save(texmap, TEXMAP)

# generate cs code
# tile positions
print('\n/!\ Don\'t forget to use .png images\n')
print('Place these instructions inside Game.cs:')
code = '\nprivate const int TexWidth = %d, TexHeight = %d;' %(w, h)
code += '\n\npublic static readonly Tile'
for i, name in zip(range(l), images):
    code += '\n    %s = new(new Vector2(%d, %d)),' %(name, i%w, h-1-i//w)
print(code[:-1]+';\n/!\\', end='')
print(' - Don\'t forget to move the texmap.png file into the Assets folder!')
input(' - Don\'t forget to update the schematic editor!')
