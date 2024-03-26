from os import listdir
from os.path import basename, join, isfile, isdir, splitext

def clean_file(filename):
    if splitext(filename)[1] != '.cs': return
    print('  - Cleaning file', basename(filename))

    with open(filename, encoding='utf-8') as f: content = f.read()

    edited = ''
    prev = None
    for line in content.split('\n'):
        line = line.rstrip()
        if line == prev == '': continue
        edited += line+'\n'
        prev = edited
    print(edited)
    djsbhdfh

def clean_folder(path):
    print('Cleaning folder', path)
    for elt in listdir(path):
        if elt in ignore: continue
        elt = join(path, elt)
        if isdir(elt): clean_folder(elt)
        elif isfile(elt): clean_file(elt)

ignore = ('.', '..', '.git')
clean_folder('../Assets/Scripts')
clean_folder('.')
