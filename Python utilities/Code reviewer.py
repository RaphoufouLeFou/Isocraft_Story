from os import listdir
from os.path import basename, join, isfile, isdir, splitext

try:
    from colorama import init, Fore
    init()
except:
    print('/!\\ Please install (`pip install colorama`) colorama')
    class Fore: GREEN = RESET = RED = YELLOW = LIGHTGREEN_EX = \
                LIGHTYELLOW_EX = LIGHTRED_EX = ''

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
    for name in ignore_filenames:
        if name in filename: return # skip this file

    if ext not in types: return

    py = 'py' in ext
    length = 80 if py else 120

    print('  - Cleaning file %s%s%s' %(
        Fore.LIGHTYELLOW_EX, basename(filename), Fore.RESET)
    )

    with open(filename, encoding='utf-8') as f:
        content = f.read().split('\n')

    edited = ''
    Prev = None
    for i, line in enumerate(content):
        # remove trailing spaces
        changed = line != line.rstrip()
        line = line.rstrip()

        add = True
        next = None if i == len(content)-1 else content[i+1].rstrip()

        # remove empty lines, except after } and before blocks
        # and also after {, and before } too
        startprev = Prev is None or Prev in '\ufeff' or Prev.lstrip() == '{'
        endnext = next is None or next == '' or next.lstrip() == '}'

        if line == '' and (py and endnext or startprev or endnext): add = False

        # add a single empty line after } and before blocks if missing
        elif Prev and Prev.lstrip() == '}' and \
             (line and line.lstrip() not in '{}'):
            edited += '\n'
            n_changes += 1

        if add:
            # add line if needed
            edited += line+'\n'
            if changed: n_changes += 1

            # group empty blocks into "{ }"
            if line.lstrip() == '}' and Prev.lstrip() == '{':
                edited = '\n'.join(edited.split('\n')[:-3]) + ' { }\n'
                n_changes += 3

            Prev = line

        elif next is not None: n_changes += 1

    # other warnings
    in_comment = False
    for i, line in enumerate(edited.split('\n')):
        next_in_comment = in_comment
        if '/*' in line:
            next_in_comment = True
            line = line.split('/*')[0]
        elif '*/' in line:
            next_in_comment = False
            line = line.split('*/')[-1]

        if len(line) > length:
            long.append(Thing(filename, i+1, line))
        if not py:
            # get code part of line, check for string additions
            _line = ''
            operation = False
            count = 0
            in_string = False
            depth = 0
            prev = None
            dollar = False
            for j, char in enumerate(line):
                if char == '"' and prev != '\\': # I know about "\\"
                    in_string = not in_string
                    count += 1
                    _line += char
                    dollar = in_string and prev == '$'
                    if in_string: depth = 0
                elif in_string: # in string: if $"", add code inside
                    if dollar and prev != '\\':
                        if char == '{': depth += 1
                        elif char == '}':
                            depth -= 1
                            _line += char
                        if depth: _line += char
                else: # outside of string: add and check for ""+""
                    _line += char
                    if char == '+' and not (
                       (j < len(line)-1 and line[j+1] in '+=') or
                       (j and line[j-1] == '+')):
                        operation = True

                prev = char

            if count > 1 and operation:
                string.append(Thing(filename, i+1, line))

            # check for operators
            ok = True
            _line = _line.split('//')[0]+' ' # I know about "// //"
            j = 1
            while j < len(_line)-2:
                if 'case' in line or 'default' in line: break
                if _line[j] in operators:
                    double = False
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

                    # avoid things like "List<(int, Vector3)>()"
                    if (_line[j] in '<>?' and not double
                        or _line[j] == '-' or _line[j:j+2] == ':F') \
                        and (a.isalpha() or a in ' 23)(]') \
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

            for j in range(1, len(_line)):
                # check commas "[no space], " except for e.g. "int[,,]"
                if j < len(_line)-1 and _line[j] == ',' and (
                   _line[j-1] == ' ' or _line[j+1] not in ' ,]' or \
                   j < len(_line)-2 and _line[j+1] == _line[j+2] == ' '):
                    ok = False
                    break

                # check semicolons "[no space];"
                if _line[j] == ';' and _line[j-1] == ' ':
                    ok = False
                    break

            # check for double spaces
            if not in_comment and '  ' in _line.lstrip()[:-1]: ok = False

            # check newline before "{", except if "{ whatever"
            _line = line.replace(' ', '')
            if len(_line) > 1 and _line[-1] == '{' and _line[0] != '{':
                ok = False

            if not ok: visual.append(Thing(filename, i+1, line))

            if 'todo' in line.lower():
                todo.append(Thing(filename, i+1, line))

            if 'GameObject.Find(' in line or 'GetComponent' in line:
                    expensive.append(Thing(filename, i+1, line))

        in_comment = next_in_comment

    if edited and edited[0] == '\ufeff':
        n_changes += 1
        edited = edited[1:]
    with open(filename, 'w', encoding='utf-8') as f: f.write(edited)

def clean_folder(path):
    for name in ignore_folders:
        if name in path: return # ignore folder

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
ignore_filenames = ('FastNoiseLite', 'Mirror')
ignore_folders = ('TextMesh',)
types = ('.cs', '.py', '.pyw')
operators = '+-*/<>=&|^:?'
double_operators = ('&&', '||', '^^', '<<', '>>', '=>', '??')
opening, closing = '([{', ')]}'

n_changes = 0
string = []
long = []
visual = []
todo = []
expensive = []

print('%sPHASE 1 - CODE CLEANING%s\n' %(Fore.RED, Fore.RESET))
clean_folder('../Assets')
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
print('%sSpaces around operators, newline %s{%s (except when inline),' %(
    Fore.LIGHTGREEN_EX, Fore.GREEN, Fore.LIGHTGREEN_EX)
)
print('no double spaces, correct comma and semicolon spacing%s\n' %Fore.RESET)
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
print('Made %s%d%s basic syntax improvements' %(
    Fore.LIGHTRED_EX if n_changes else Fore.GREEN, n_changes, Fore.RESET)
)

input()
