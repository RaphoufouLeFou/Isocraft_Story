from os import listdir
from os.path import join, splitext, isfile, isdir, basename

from tkinter import *
from tkinter.ttk import *
from tkinter.scrolledtext import *

ext = ('.cs', '.py', '.pyw')
width = 100

def find_files(path):
    add = []
    for file in listdir(path):
        file = join(path, file)
        if isfile(file) and splitext(file)[1] in ext: add.append(file)
        elif isdir(file): add += find_files(file)
    return add

def find_all_files():
    global files
    files = find_files('../Assets/Scripts')

def search():
    # get info in widgets
    expression = entry.get()
    if not expression: return

    casing, whole_word = check[0].get(), check[1].get()
    if not expression.isalnum(): whole_word = False
    if not casing: expression = expression.lower()

    # refresh files list and prepare widget
    find_all_files()
    List.delete(1.0, END)
    List.yview = 0
    count = 0

    # perform the search
    for file in files:
        # syntax: (file name, line index, line string, index of find)
        found = []

        with open(file, encoding='utf-8') as f: content = f.read().split('\n')
        for i, line in enumerate(content):
            if not casing: line = line.lower()

            if whole_word:
                current = ''
                prev = None # 0: other character, 1: alphanumeric
                for j, char in enumerate(line):
                    now = char.isalnum()
                    if now:
                        current += char
                    else:
                        if current == expression:
                            found.append((file, i, line, j))
                            break
                        current = ''
                    prev = now
                if current == expression:
                    found.append((file, i, line, j))
            else:
                if expression in line:
                    found.append((file, i, line, line.index(expression)))

        # edit the list file by file to refresh as the search progresses
        for a, b, c, d in found:
            text = 'In "%s", line %d: ' %(basename(a), b)
            length = width-len(text)

            # cut the line and add dots if needed,
            # to never have the found expression offscreen
            if len(c) > length:
                if d+len(expression) > length: # found expression is offscreen
                    c = c[min(d+len(expression)-length+20, len(c)-length):]

                    # add dots
                    c = '...'+c[3:]
                    if len(c) > length: c = c[:length-3]+'...'
                elif len(c) > length: c = c[:length-3]+'...'

            text += c

            if count: text = '\n'+text
            count += 1

            # colorize text

            # add result to list
            List.insert(END, text)

def make_win():
    global tk, entry, check, List

    # window settings
    tk = Tk()
    tk.resizable(0, 0)
    tk.title('Search')

    # entry
    entry = Entry(tk, width=50, font=('Consolas', 9))
    entry.grid(padx=(5, 0), pady=5)
    entry.focus()

    # add frame to group buttons
    frame = Frame(tk)

    # options checkboxes
    check = [BooleanVar(), BooleanVar()]
    cb = Checkbutton(frame, text='Respect casing', variable=check[0])
    cb.grid(column=1, row=0, padx=(0, 5))
    cb = Checkbutton(frame, text='Whole word', variable=check[1])
    cb.grid(column=2, row=0, padx=(0, 5))

    # search button
    Button(frame, text='Search', command=search).grid(column=3, row=0)

    frame.grid(column=1, row=0, padx=5, pady=(5, 0))

    # list frame
    frame = Frame(tk)

    # main list widget
    listvar = StringVar()
    List = Text(frame, width=width, height=20, font=('Consolas', 9))
    List.grid()

    # set up tags for text coloring
    List.tag_config('blue', foreground='blue')
    List.tag_config('red', foreground='red')

    # list scrollbar
    scrollbar = Scrollbar(frame, command=List.yview)
    List.configure(yscroll=scrollbar.set)
    scrollbar.grid(column=1, row=0, sticky='ns')

    frame.grid(columnspan=2, padx=5, pady=5)

make_win()
tk.mainloop()
