from os import listdir
from os.path import basename, join, isfile, isdir, splitext
from colorama import init, Fore

init()

class Thing:
    def __init__(self, *args):
        self.file, self.line, self.code = args
        self.code = self.code.strip()
        if len(self.code) > 120: self.code = self.code[:116]+'...'
    def __repr__(self):
        return 'In file %s"%s"%s, line %s%d%s:\n%s%s%s' %(
            Fore.GREEN, self.file, Fore.RESET, Fore.GREEN, self.line,
            Fore.RESET, Fore.RED, self.code, Fore.RESET)

def clean_file(filename):
    global n_changes, string, long, visual, todo, expensive

    ext = splitext(filename)[1]
    if 'FastNoiseLite' in filename: return # skip this file
    if ext not in types: return

    py = 'py' in ext
    length = 80 if py else 120

    print('  - Cleaning file %s%s%s' %(
        Fore.LIGHTYELLOW_EX, basename(filename), Fore.RESET)
    )

    with open(filename, encoding='utf-8') as f: content = f.read().split('\n')

    edited = ''
    prev = None
    for i, line in enumerate(content):
        line = line.rstrip()

        add = True
        next = None if i == len(content)-1 else content[i+1].rstrip()

        endnext = next is None or next == '' or next[-1] == '}'
        if line == prev == '': add = False
        elif line == '' and (prev[-1] == '}' or py) and endnext: add = False

        if add:
            edited += line+'\n'

            if len(line) > length:
                long.append(Thing(filename, i+1, line))
            if not py:
                # get code part of line, check for string additions
                _line = ''
                operation = []
                count = 0
                in_string = False
                depth = 0
                _depth = 0
                prev = None
                dollar = False
                for char in line:
                    if char == '"' and prev != '\\': # I know about \\
                        in_string = not in_string
                        count += 1
                        _line += char
                        dollar = in_string and prev == '$'
                        if in_string: _depth = 0
                    elif in_string: # in string: if $"", add code inside
                        if dollar and prev != '\\':
                            if char in opening: _depth += 1
                            elif char in closing:
                                _depth -= 1
                                _line += char
                            if _depth: _line += char
                    else: # outside of string: add and check for ""+""
                        _line += char

                        if char in opening: depth += 1
                        elif char in closing: depth -= 1
                        elif not depth and char == '+':
                            operation.append(count)

                    prev = char

                op = False
                for o in operation:
                    if o != 0 and o != count:
                        op = True
                        break

                if op: string.append(Thing(filename, i+1, line))

                # check for operators
                ok = True
                _line = _line.split('//')[0]+' ' # I know about // //
                j = 1
                while j < len(_line)-2:
                    if 'case' in line: break
                    if _line[j] in operators:
                        a, b = j-1, j+1
                        # can extend operator if made of multiple characters
                        if b < len(_line)-1:
                            if _line[j:j+2] in ('++', '--'):
                                j += 2
                                continue
                            double = _line[j:j+2] in double_operators
                            if double or _line[b] == '=':
                                j += 1
                                b += 1
                                if b < len(_line)-1 and double \
                                   and _line[j+1] == '=':
                                    # triple characters operators
                                    j += 1
                                    b += 1

                        if a and _line[a] == '!': a -= 1
                        a, b = _line[a], _line[b]
                        if a == b == "'":
                            j += 1
                            continue # char

                        # avoid things like List<(int, Vector3)>()
                        if (a.isalpha() or a in ' 23)(]') \
                           and (b.isalnum() or b == ' '
                            or _line[j+1:j+3] == '()' or (
                            b == '(' and _line[j+2].isalpha())):
                            j += 1
                            continue

                        if not (a == 0 or a == ' ') or not (
                            b == len(_line)-1 or b == ' '):
                            ok = False
                            break
                    j += 1

                _line = line.replace(' ', '')
                if len(_line) > 1 and _line[-1] == '{' and _line[0] != '{':
                    ok = False

                if not ok: visual.append(Thing(filename, i+1, line))

                if 'TODO' in line: todo.append(Thing(filename, i+1, line))

                if 'GameObject.Find(' in line or 'GetComponent' in line:
                        expensive.append(Thing(filename, i+1, line))

        elif next is not None: n_changes += 1
        prev = line

    with open(filename, 'w', encoding='utf-8') as f: f.write(edited)

def clean_folder(path):
    print('%sLooking in %s%s%s' %(
        Fore.LIGHTYELLOW_EX, Fore.YELLOW, basename(path), Fore.RESET)
    )
    for elt in listdir(path):
        if elt in ignore: continue
        elt = join(path, elt)
        if isdir(elt): clean_folder(elt)
        elif isfile(elt): clean_file(elt)

def title(name):
    print('\n%s-----%s-----%s' %(Fore.YELLOW, name, Fore.RESET))

ignore = ('.', '..', '.git')
types = ('.cs', '.py', '.pyw')
operators = '+-*/<>=&|^:?'
double_operators = ('&&', '||', '^^', '<<', '>>', '=>', '??', '/*', '*/')
opening, closing = '([{', ')]}'

n_changes = 0
string = []
long = []
visual = []
todo = []
expensive = []

print('%sPHASE 1 - CODE CLEANING%s\n' %(Fore.RED, Fore.RESET))
clean_folder('../Assets/Scripts')
clean_folder('.')

print()
print('%s\nPHASE 2 - FEEDBACK%s' %(Fore.RED, Fore.RESET))

title('EXPENSIVE METHODS')
print('Reduce expensive method invocations usage')
for thing in expensive: print(thing)
n = len(expensive)
print('Detected %s%d%s issues' %(
    Fore.LIGHTRED_EX if n else Fore.GREEN, n, Fore.RESET)
)

title('STRING OPERATIONS')
print('%sUse %s$""%s for compactness%s\n' %(
    Fore.LIGHTGREEN_EX, Fore.GREEN, Fore.LIGHTGREEN_EX, Fore.RESET)
)
for thing in string: print(thing)
n = len(string)
print('Detected %s%d%s issues' %(
    Fore.LIGHTRED_EX if n else Fore.GREEN, n, Fore.RESET)
)

title('LONG LINES')
print('don\'t exceed 120(cs)/80(py) characters per line')
for thing in long: print(thing)
n = len(long)
print('Detected %s%d%s issues' %(
    Fore.LIGHTRED_EX if n else Fore.GREEN, n, Fore.RESET)
)

title('VISUAL')
print('%sSpaces around operators, newline %s{%s (except when inline)%s\n' %(
    Fore.LIGHTGREEN_EX, Fore.GREEN, Fore.LIGHTGREEN_EX, Fore.RESET)
)
for thing in visual: print(thing)
n = len(visual)
print('Detected %s%d%s issues' %(
    Fore.LIGHTRED_EX if n else Fore.GREEN, n, Fore.RESET)
)

title('TODO')
print('%sDo every %sTODO%s\n' %(
    Fore.LIGHTGREEN_EX, Fore.GREEN, Fore.RESET)
)
for thing in todo: print(thing)
n = len(todo)
print('Detected %s%d%s issues' %(
    Fore.LIGHTRED_EX if n else Fore.GREEN, n, Fore.RESET)
)

title('COMPACTNESS')
print('Made %s%d%s changes in basic syntax compactness' %(
    Fore.LIGHTRED_EX if n_changes else Fore.GREEN, n_changes, Fore.RESET)
)

input()
