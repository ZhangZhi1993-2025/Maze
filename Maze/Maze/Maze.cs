using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PriorityQueue;

namespace Maze
{
    public partial class Maze : Form
    {
        #region params to generate a maze
        private static int N = 12;//scale of maze
        private bool[,] map = new bool[N * N, 5];//logic map of maze
        private bool[,] choice = new bool[N * N, 5];// ways for a unit to choose
        private Point[,] mapPoint = new Point[N + 1, N + 1];//physic map of maze
        private Stack<int> gStack = new Stack<int>();//the stack to record the explored units
        private int seed = -1;
        private int seedLatch = -1;//if you want to recover from a damaged maze
        #endregion

        #region params to explore a maze
        private Point[,] path = new Point[N, N];//the key to the maze
        private Stack<int> rStack = new Stack<int>();//the stack to save the current path
        #endregion

        public Maze()
        {
            InitializeComponent();
        }

        //alg. to generate the maze
        private void generate(object sender, EventArgs e)
        {
            #region initialize
            for (int i = 0; i < N * N; i++)
                for (int j = 0; j < 5; j++)
                {
                    map[i, j] = false;//0 for stone and 1 for access
                    choice[i, j] = false;
                }
            map[0, 4] = true;         //entrance
            map[N * N - 1, 2] = true; //exit
            Random r;
            if (seed == -1)
            {
                seedLatch = DateTime.Now.Millisecond;
                r = new Random(seedLatch);
            }
            else
                r = new Random(seed);
            int now = r.Next(0, N * N);
            map[now, 0] = true;
            gStack.Clear();
            gStack.Push(now);
            int direction = 0;
            int choiceCount = 0;
            #endregion

            #region the main part of the algorithm
            while (gStack.Count > 0)
            {
                if ((choiceCount = adjacent(now)) > 0)//the unit now has adjacent units to be explored
                {
                    direction = r.Next(1, choiceCount + 1);
                    for (int i = 1; i < 5; i++)
                    {
                        if (choice[now, i] == true && --direction == 0)
                        {
                            gStack.Push(now);
                            map[now, i] = true;
                            switch (i)
                            {
                                case 1:
                                    map[now + 1, 3] = true;
                                    now = now + 1;
                                    break;
                                case 2:
                                    map[now + N, 4] = true;
                                    now = now + N;
                                    break;
                                case 3:
                                    map[now - 1, 1] = true;
                                    now = now - 1;
                                    break;
                                case 4:
                                    map[now - N, 2] = true;
                                    now = now - N;
                                    break;
                                default: break;
                            }
                            break;
                        }
                        else continue;
                    }
                    map[now, 0] = true;
                }
                else
                {
                    now = gStack.Peek();
                    gStack.Pop();
                }
            }
            #endregion

            #region GDI engine: drawing the maze
            Graphics g = this.CreateGraphics();
            Pen p = new Pen(Color.Black, 3);
            g.Clear(Color.White);
            for (int i = 0; i < N + 1; i++)
                for (int j = 0; j < N + 1; j++)
                {
                    mapPoint[i, j] = new Point(j * 40 + 150, i * 40 + 40);
                }
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                {
                    if (map[N * i + j, 4] == false)
                        g.DrawLine(p, mapPoint[i, j], mapPoint[i, j + 1]);
                    if (map[N * i + j, 3] == false)
                        g.DrawLine(p, mapPoint[i, j], mapPoint[i + 1, j]);
                }
            for (int i = 0; i < N; i++)
            {
                if (map[N * (i + 1) - 1, 1] == false)
                    g.DrawLine(p, mapPoint[i, N], mapPoint[i + 1, N]);
                if (map[N * (N - 1) + i, 2] == false)
                    g.DrawLine(p, mapPoint[N, i], mapPoint[N, i + 1]);
            }
            #endregion

            //a security measure
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            seed = -1;
        }

        //to judge how many ways can the unit now to explore(used in generate alg.)
        private int adjacent(int now)
        {
            int count = 0;
            if (now % N < N - 1 && map[now + 1, 0] == false)
            {
                choice[now, 1] = true;
                count++;
            }
            else choice[now, 1] = false;
            if (now < N * (N - 1) && map[now + N, 0] == false)
            {
                choice[now, 2] = true;
                count++;
            }
            else choice[now, 2] = false;
            if (now % N != 0 && map[now - 1, 0] == false)
            {
                choice[now, 3] = true;
                count++;
            }
            else choice[now, 3] = false;
            if (now > N - 1 && map[now - N, 0] == false)
            {
                choice[now, 4] = true;
                count++;
            }
            else choice[now, 4] = false;
            return count;
        }

        private void DFS(object sender, EventArgs e)
        {
            rStack.Clear();
            //temporary eStack(expandStack) to record which node to be expanded
            Stack<int> eStack = new Stack<int>();
            rStack.Push(-N);//we suppose that the "previous unit" before the start unit is -N
            eStack.Push(0);
            while (rStack.Peek() != N * N - 1)
            {
                int now = eStack.Peek();
                while (now == -1)
                {
                    rStack.Pop();
                    eStack.Pop();
                    now = eStack.Peek();
                }
                eStack.Pop();
                eStack.Push(-1);
                for (int i = 1; i < 5; i++)
                {
                    if (map[now, i] == true)
                    {
                        int temp = 0;
                        switch (i)
                        {
                            case 1:
                                temp = now + 1;
                                break;
                            case 2:
                                temp = now + N;
                                break;
                            case 3:
                                temp = now - 1;
                                break;
                            case 4:
                                temp = now - N;
                                break;
                            default: break;
                        }
                        if (temp == rStack.Peek())
                            continue;
                        eStack.Push(temp);
                    }
                }
                if (eStack.Peek() != -1)
                    rStack.Push(now);
                else
                    eStack.Pop();
            }
            DrawKey();
        }

        //used in BFS and A_Star
        public struct Node : IComparable<Node>
        {
            public int CompareTo(Node another)
            {
                int length1 = N - this.unit % N;
                int width1 = N - this.unit / N;
                int length2 = N - another.unit % N;
                int width2 = N - another.unit / N;
                return length1 + width1 - length2 - width2;
            }

            public Node(int parent, int unit, int parentUnit)
            {
                this.parent = parent;
                this.unit = unit;
                this.parentUnit = parentUnit;
            }

            public int parent;
            public int unit;
            public int parentUnit;   //trading space for time
        };

        private void BFS(object sender, EventArgs e)
        {
            //temporary oQueue(openQueue) to record which node to be expanded
            Queue<Node> queue = new Queue<Node>();
            //temporary cList(closedQueue) to record the expanded nodes
            System.Collections.ArrayList list = new System.Collections.ArrayList();
            queue.Enqueue(new Node(0, 0, -N));
            list.Add(new Node(-1, -N, -1));
            while (true)
            {
                Node node = queue.Peek();
                list.Add(node);
                queue.Dequeue();
                int parent = list.Count - 1;
                int unit = node.unit;
                for (int i = 1; i < 5; i++)
                {
                    if (map[unit, i] == true)
                    {
                        int temp = 0;
                        switch (i)
                        {
                            case 1:
                                temp = unit + 1;
                                break;
                            case 2:
                                temp = unit + N;
                                break;
                            case 3:
                                temp = unit - 1;
                                break;
                            case 4:
                                temp = unit - N;
                                break;
                            default: break;
                        }
                        if (temp == node.parentUnit)
                            continue;
                        else if (temp == N * N - 1)
                        {
                            list.Add(new Node(parent, temp, unit));
                            goto End;
                        }
                        queue.Enqueue(new Node(parent, temp, unit));
                    }
                }
            }
        End:
            {
                rStack.Clear();
                rStack.Push(-N);
                Stack<int> tempStack = new Stack<int>();
                int index=list.Count - 1;
                while (index > -1)
                {
                    Node node = (Node)list[index];
                    tempStack.Push(node.unit);
                    index = node.parent;
                }
                while (tempStack.Count > 0)
                {
                    rStack.Push(tempStack.Peek());
                    tempStack.Pop();
                }
            }
            DrawKey();
        }

        private void A_Star(object sender, EventArgs e)
        {
            //temporary prioirtyQueue to record which node to be expanded
            PriorityQueue<Node> queue = new PriorityQueue<Node>();
            //temporary cList(closedQueue) to record the expanded nodes
            System.Collections.ArrayList list = new System.Collections.ArrayList();
            queue.Enqueue(new Node(0, 0, -N));
            list.Add(new Node(-1, -N, -1));
            while (true)
            {
                Node node = queue.Dequeue();
                list.Add(node);
                int parent = list.Count - 1;
                int unit = node.unit;
                for (int i = 1; i < 5; i++)
                {
                    if (map[unit, i] == true)
                    {
                        int temp = 0;
                        switch (i)
                        {
                            case 1:
                                temp = unit + 1;
                                break;
                            case 2:
                                temp = unit + N;
                                break;
                            case 3:
                                temp = unit - 1;
                                break;
                            case 4:
                                temp = unit - N;
                                break;
                            default: break;
                        }
                        if (temp == node.parentUnit)
                            continue;
                        else if (temp == N * N - 1)
                        {
                            list.Add(new Node(parent, temp, unit));
                            goto End;
                        }
                        queue.Enqueue(new Node(parent, temp, unit));
                    }
                }
            }
        End:
            {
                rStack.Clear();
                rStack.Push(-N);
                Stack<int> tempStack = new Stack<int>();
                int index = list.Count - 1;
                while (index > -1)
                {
                    Node node = (Node)list[index];
                    tempStack.Push(node.unit);
                    index = node.parent;
                }
                while (tempStack.Count > 0)
                {
                    rStack.Push(tempStack.Peek());
                    tempStack.Pop();
                }
            }
            DrawKey();
        }

        //GDI engine: drawing the key to the maze
        private void DrawKey()
        {
            Graphics g = this.CreateGraphics();
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                {
                    path[i, j] = new Point(j * 40 + 170, i * 40 + 60);
                }
            Pen p = new Pen(Color.Red, 5);
            Point pre = path[N - 1, N - 1];
            rStack.Pop();
            Point post;
            int next = 0;
            int x = 0, y = 0;
            while ((next = rStack.Peek()) != -N)
            {
                rStack.Pop();
                x = next / N;
                y = next % N;
                post = path[x, y];
                g.DrawLine(p, pre, post);
                pre = post;
            }
            g.DrawLine(p, path[0, 0], new Point(170, 40));
            g.DrawLine(p, path[N - 1, N - 1], new Point((N - 1) * 40 + 170, (N - 1) * 40 + 80));
        }

        private void recovery(object sender, EventArgs e)
        {
            seed = seedLatch;
            generate(sender, e);
        }
    }
}