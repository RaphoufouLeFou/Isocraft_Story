from os import listdir
from os.path import basename, join, isfile, isdir, splitext

def clean_file(filename):
    ext = splitext(filename)[1]
    if ext not in types: return
    py = 'py' in ext
    print('  - Cleaning file', basename(filename))

    with open(filename, encoding='utf-8') as f: content = f.read().split('\n')

    edited = ''
    prev = None
    for i, line in enumerate(content):
        line = line.rstrip()

        add = True
        next = '' if i == len(content)-1 else content[i+1].rstrip()

        endnext = next == '' or next[-1] == '}'
        if line == prev == '': add = False
        elif line == '' and (prev[-1] == '}' or py) and endnext: add = False

        if add: edited += line+'\n'
        prev = line

    with open(filename, 'w', encoding='utf-8') as f: f.write(edited)

def clean_folder(path):
    print('Cleaning folder', path)
    for elt in listdir(path):
        if elt in ignore: continue
        elt = join(path, elt)
        if isdir(elt): clean_folder(elt)
        elif isfile(elt): clean_file(elt)

ignore = ('.', '..', '.git')
types = ('.cs', '.py', '.pyw')

clean_folder('../Assets/Scripts')
clean_folder('.')
