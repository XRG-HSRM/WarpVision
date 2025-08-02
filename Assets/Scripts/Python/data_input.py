import socket
import tkinter as tk
from tkinter import messagebox
import threading
from tkinter import ttk
import csv
import os

server_socket = None
conn = None

def main():
    global server_socket
    host = '127.0.0.1'
    port = 9999
    
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    
    try:
        server_socket.bind((host, port))
        server_socket.listen(1)
        print(f"Listening on {host}:{port}...")
        
        while True:
            conn, addr = server_socket.accept()
            print(f"Connection established from {addr}")
            client_thread = threading.Thread(target=handle_client, args=(conn,))
            client_thread.start()
    
    except Exception as e:
        print(f"Error starting server: {e}")
        if server_socket:
            server_socket.close()

def handle_client(conn):
    global question_window, quadrant_var, saw_anything_var
    
    with conn:
        while True:
            try:
                data = conn.recv(1024).decode('utf-8')
                
                if not data:
                    break
                
                print(f"Received: {data}")
                if data.strip() == "paused":
                    conn.sendall("received pause.".encode('utf-8'))
                    show_questionnaire(conn)
                elif(data.strip() == "start"):
                    conn.sendall("received start.".encode('utf-8'))
                    show_intro_questions(conn)
                elif("endq" in data.strip()):
                    rec_data = data.strip()
                    conn.sendall("received endq.".encode('utf-8'))
                    values = rec_data.split('_')
                    id_value = values[1]
                    type_value = values[2]
                    print(rec_data)
                    print(id_value)
                    show_discomfort_questionnaire(conn, id_value, type_value)
                else:
                    print("Unknown message received")
            
            except Exception as e:
                print(f"Error handling client: {e}")
                break

def show_intro_questions(conn):
    root = tk.Tk()
    root.title("Questionnaire")
    root.geometry("600x540")
    root.focus_force()
    prefix = "tostart:"

    def on_submit():
        age_value = age_var.get()
        gender_value = gender_var.get()
        vr_experience_value = vr_experience_var.get()
        eye_impairment_value = eye_impairment_var.get()

        if not age_value or not gender_value or not vr_experience_value or not eye_impairment_value:
            messagebox.showerror("Error", "All questions must be answered")
            return

        text_to_send = f"{prefix}{age_value};{gender_value};{vr_experience_value};{eye_impairment_value}"
        conn.sendall(text_to_send.encode('utf-8'))
        root.destroy()

    frame = ttk.Frame(root, padding="20")
    frame.pack(expand=True, fill='both')

    # Question 1
    tk.Label(frame, text="Wie alt sind Sie?", font=("Arial", 12)).pack(anchor=tk.W)
    age_var = tk.StringVar()
    tk.Entry(frame, textvariable=age_var).pack(anchor=tk.W, padx=10, fill='x')

    # Question 2
    tk.Label(frame, text="Welches Geschlecht?", font=("Arial", 12)).pack(anchor=tk.W)
    gender_var = tk.StringVar()

    ttk.Radiobutton(frame, text="Männlich", variable=gender_var, value="m").pack(anchor=tk.W, padx=10, fill='x')
    ttk.Radiobutton(frame, text="Weiblich", variable=gender_var, value="w").pack(anchor=tk.W, padx=10, fill='x')
    ttk.Radiobutton(frame, text="Diverse", variable=gender_var, value="d").pack(anchor=tk.W, padx=10, fill='x')

    # Question 3
    tk.Label(frame, text="Kennen Sie sich mit VR aus? (1=überhaupt nicht, 5=sehr gut)", font=("Arial", 12)).pack(anchor=tk.W)
    vr_experience_var = tk.StringVar()

    for i in range(1, 6):
        ttk.Radiobutton(frame, text=str(i), variable=vr_experience_var, value=str(i)).pack(anchor=tk.W, padx=10, fill='x')

    # Question 4
    tk.Label(frame, text="Gibt es Beeinträchtigungen des Auges?", font=("Arial", 12)).pack(anchor=tk.W)
    eye_impairment_var = tk.StringVar()

    ttk.Radiobutton(frame, text="Keine", variable=eye_impairment_var, value="none").pack(anchor=tk.W, padx=10, fill='x')
    ttk.Radiobutton(frame, text="Kurzsichtig", variable=eye_impairment_var, value="shortsighted").pack(anchor=tk.W, padx=10, fill='x')
    ttk.Radiobutton(frame, text="Weitsichtig", variable=eye_impairment_var, value="longsighted").pack(anchor=tk.W, padx=10, fill='x')
    ttk.Radiobutton(frame, text="Andere", variable=eye_impairment_var, value="other").pack(anchor=tk.W, padx=10, fill='x')

    submit_button = ttk.Button(frame, text="Submit", command=on_submit)
    submit_button.pack(pady=10)

    root.mainloop()

def show_questionnaire(conn):
    root = tk.Tk()
    root.title("Questionnaire")
    root.geometry("600x540")
    root.focus_force()
    prefix = "tolog:"

    def on_submit():
        question1_value = question1_var.get()
        question2_value = question2_var.get()

        if not question1_value or not question2_value:
            messagebox.showerror("Error", "Alle Fragen müssen beantwortet werden")
            return
        
        text_to_send = f"{prefix}{question1_value};{question2_value}"
        conn.sendall(text_to_send.encode('utf-8'))
        root.destroy()

    frame = ttk.Frame(root, padding="20")
    frame.pack(expand=True, fill='both')

    # Question 1
    tk.Label(frame, text="Mir ist aufgefallen, dass ich im Suchen unterstützt wurde (1=gar nicht, 5=sehr)", font=("Arial", 12)).pack(anchor=tk.W)
    question1_var = tk.StringVar()

    for i in range(0, 6):
        ttk.Radiobutton(frame, text=str(i), variable=question1_var, value=str(i)).pack(anchor=tk.W, padx=10, fill='x')

    # Question 2
    tk.Label(frame, text="Die unterstützende Technik hat meine Immersion gestört (1=gar nicht, 5=sehr)", font=("Arial", 12)).pack(anchor=tk.W)
    question2_var = tk.StringVar()

    for i in range(0, 6):
        ttk.Radiobutton(frame, text=str(i), variable=question2_var, value=str(i)).pack(anchor=tk.W, padx=10, fill='x')

    submit_button = ttk.Button(frame, text="Submit", command=on_submit)
    submit_button.pack(pady=10)

    def update_radiobutton(event):
        key = event.char
        if question1_var.get() and question2_var.get():
            question1_var.set('')
            question2_var.set('')
        if key in '012345':
            if not question1_var.get():
                question1_var.set(key)
            elif not question2_var.get():
                question2_var.set(key)

    root.bind('<Key>', update_radiobutton)
    root.bind('<Return>', lambda event: submit_button.invoke())

    root.mainloop()

def show_discomfort_questionnaire(conn, user_id, catcher_type):
    root = tk.Tk()
    root.title("Discomfort Questionnaire")
    root.geometry("1200x560")
    root.focus_force()
    prefix = 'resumeendq'
    questions = [
        "1. Allgemeines Unbehagen", 
        "2. Erschoepfung", 
        "3. Kopfschmerzen", 
        "4. Augenbelastung", 
        "5. Konzentrationsschwierigkeiten", 
        "6. Druck im Kopf", 
        "7. Verschwommenes Sehen", 
        "8. Schwindel bei geschlossenen Augen", 
        "9. Schwindel"
    ]
    def on_submit():
        question_values = [question_vars[i].get() for i in range(len(question_vars))]
        if any(val == '' for val in question_values):
            messagebox.showerror("Error", "Alle Fragen müssen beantwortet werden")
            return

        file_exists = os.path.isfile('../../Data/cybersickness_questionnaire.csv')
        with open('../../Data/cybersickness_questionnaire.csv', mode='a', newline='', encoding='utf-8') as file:
            writer = csv.writer(file, delimiter=';')
            if not file_exists:
                writer.writerow(['ID'] + ['type'] + questions)
            writer.writerow([user_id] + [catcher_type] + question_values)
        conn.sendall(prefix.encode('utf-8'))
        root.destroy()

    question_vars = [tk.StringVar() for _ in questions]
    frame = ttk.Frame(root, padding="20")
    frame.pack(expand=True, fill='both')
    for idx, question in enumerate(questions):
        question_label = tk.Label(frame, text=question, font=("Arial", 12))
        question_label.grid(row=idx, column=0, padx=10, pady=5, sticky=tk.W)

        for i, label in enumerate(["None", "Slight", "Moderate", "Severe"]):
            ttk.Radiobutton(frame, text=f"{i} = {label}", variable=question_vars[idx], value=str(i)).grid(row=idx, column=i + 1, padx=10, pady=5)
    submit_button = ttk.Button(frame, text="Submit", command=on_submit)
    submit_button.grid(row=len(questions), column=0, columnspan=5, pady=20)
    def update_radiobutton(event):
        key = event.char 
        if key in '0123':
            for var in question_vars:
                if not var.get():
                    var.set(key)
                    break

    root.bind('<Key>', update_radiobutton)
    root.bind('<Return>', lambda event: submit_button.invoke())

    root.mainloop()    

if __name__ == "__main__":
    main()
