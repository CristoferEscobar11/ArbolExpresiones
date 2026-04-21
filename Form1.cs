using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArbolExpresiones
{
    public partial class Form1 : Form
    {
        Nodo raiz;
        List<NodoVisual> nodosAnimados = new List<NodoVisual>();

        public Form1()
        {
            InitializeComponent();

            btnGenerar.Click += btnGenerar_Click;
            panel1.Paint += panel1_Paint;

            panel1.BackColor = Color.White;
        }

      
        private async void btnGenerar_Click(object sender, EventArgs e)
        {
            string infija = txtExpresion.Text.Replace(" ", "");
            string postfija = InfijaAPostfija(infija);

            raiz = ConstruirArbol(postfija);

            nodosAnimados.Clear();
            await GenerarAnimacion(raiz, panel1.Width / 2, 20, 120);
        }

        
        public class Nodo
        {
            public string Valor;
            public Nodo Izquierdo;
            public Nodo Derecho;

            public Nodo(string valor)
            {
                Valor = valor;
            }
        }

        
        public class NodoVisual
        {
            public Nodo Nodo;
            public int X, Y;
            public int TargetX, TargetY;
        }

       
        public static int Prioridad(char op)
        {
            if (op == '+' || op == '-') return 1;
            if (op == '*' || op == '/') return 2;
            return 0;
        }

      
        public static string InfijaAPostfija(string exp)
        {
            Stack<char> pila = new Stack<char>();
            string salida = "";

            foreach (char c in exp)
            {
                if (char.IsDigit(c)) salida += c;
                else if (c == '(') pila.Push(c);
                else if (c == ')')
                {
                    while (pila.Peek() != '(')
                        salida += pila.Pop();
                    pila.Pop();
                }
                else
                {
                    while (pila.Count > 0 && Prioridad(pila.Peek()) >= Prioridad(c))
                        salida += pila.Pop();
                    pila.Push(c);
                }
            }

            while (pila.Count > 0)
                salida += pila.Pop();

            return salida;
        }

      
        public static Nodo ConstruirArbol(string postfija)
        {
            Stack<Nodo> pila = new Stack<Nodo>();

            foreach (char c in postfija)
            {
                if (char.IsDigit(c)) pila.Push(new Nodo(c.ToString()));
                else
                {
                    Nodo der = pila.Pop();
                    Nodo izq = pila.Pop();

                    Nodo nuevo = new Nodo(c.ToString());
                    nuevo.Izquierdo = izq;
                    nuevo.Derecho = der;

                    pila.Push(nuevo);
                }
            }
            return pila.Pop();
        }

        
        private async Task GenerarAnimacion(Nodo nodo, int x, int y, int espacio)
        {
            if (nodo == null) return;

            NodoVisual nv = new NodoVisual()
            {
                Nodo = nodo,
                X = panel1.Width / 2,
                Y = 0,
                TargetX = x,
                TargetY = y
            };

            nodosAnimados.Add(nv);

            for (int i = 0; i < 20; i++)
            {
                nv.X += (nv.TargetX - nv.X) / 5;
                nv.Y += (nv.TargetY - nv.Y) / 5;
                panel1.Invalidate();
                await Task.Delay(20);
            }

            await GenerarAnimacion(nodo.Izquierdo, x - espacio, y + 80, espacio / 2);
            await GenerarAnimacion(nodo.Derecho, x + espacio, y + 80, espacio / 2);
        }

       
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            foreach (var nv in nodosAnimados)
            {
                if (nv.Nodo.Izquierdo != null)
                {
                    var hijo = nodosAnimados.Find(n => n.Nodo == nv.Nodo.Izquierdo);
                    if (hijo != null)
                        g.DrawLine(Pens.Gray, nv.X + 15, nv.Y + 30, hijo.X + 15, hijo.Y);
                }

                if (nv.Nodo.Derecho != null)
                {
                    var hijo = nodosAnimados.Find(n => n.Nodo == nv.Nodo.Derecho);
                    if (hijo != null)
                        g.DrawLine(Pens.Gray, nv.X + 15, nv.Y + 30, hijo.X + 15, hijo.Y);
                }
            }

            foreach (var nv in nodosAnimados)
            {
                g.FillEllipse(Brushes.LightSkyBlue, nv.X, nv.Y, 30, 30);
                g.DrawEllipse(Pens.Black, nv.X, nv.Y, 30, 30);
                g.DrawString(nv.Nodo.Valor, this.Font, Brushes.Black, nv.X + 10, nv.Y + 5);
            }
        }
    }
}
