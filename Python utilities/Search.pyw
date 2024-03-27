from os import listdir
from os.path import join, isfile, isdir

from tkinter import *
from tkinter.ttk import *
from tkinter.scrolledtext import *

def find_files(path):
    add = []
    for file in listdir(path):
        file = join(path, file)
        if isfile(file): add.append(file)
        elif isdir(file): add += find_files(file)
    return add

def find_all_files():
    global files
    files = find_files('../Assets/Scripts')

def search(evt):
    expression = entry.get()
    casing, whole_word = check[0].get(), check[1].get()

def make_win():
    global tk, entry, check, Text

    # window settings
    tk = Tk()
    tk.resizable(0, 0)
    tk.title('Search')

    # entry
    entry = Entry(tk, width=50, font=('Consolas', 9))
    entry.grid(padx=(5, 0), pady=5)

    # add frame to group buttons
    frame = Frame(tk)

    # options checkboxes
    check = [BooleanVar(), BooleanVar()]
    cb = Checkbutton(frame, text='Respect casing', variable=check[0])
    cb.grid(column=1, row=0, padx=(5, 0))
    cb = Checkbutton(frame, text='Whole word', variable=check[1])
    cb.grid(column=2, row=0, padx=(5, 0))

    # search button
    Button(frame, text='Search', command=search).grid(column=3, row=0, padx=5)

    frame.grid(column=1, row=0)

    # main text widget
    Text = ScrolledText(tk, width=100, height=20, font=('Consolas', 9))
    Text.grid(padx=5, columnspan=2)

make_win()
tk.mainloop()
