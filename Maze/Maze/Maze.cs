using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Maze
{
    public partial class Maze : Form
    {
        //params to generate a maze
        #region
        private static int N = 17;//scale of maze
        private bool[,] map = new bool[N * N, 5];//logic map of maze
        private bool[,] choice = new bool[N * N, 5];// ways for a unit to choose
        private Point[,] mapPoint = new Point[N + 1, N + 1];//physic map of maze
        private Stack<int> gStack = new Stack<int> { };//the stack to record the explored units
        private int seed = -1;
        private int seedLatch = -1;//if you want to recover from a damaged maze
        #endregion

        //params to explore a maze
        #region
        private Point[,] path = new Point[N, N];//the key to the maze
        private Stack<int> rStack = new Stack<int> { };//the stack to record the explored units
        #endregion

        public Maze()
        {
            InitializeComponent();
        }

        private void generate(object sender, EventArgs e)//alg. to generate the maze 
        {
            //initialize
            #region
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
            int now = 0;
            map[now, 0] = true;
            gStack.Clear();
            gStack.Push(now);
            int direction = 0;
            int choiceCount = 0;
            #endregion

            //the main part of the algorithm
            #region
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
                                    map[now + 1, 3]  = true;
                                    now = now + 1;
                                    break;
                                case 2:
                                    map[now + N, 4]  = true;
                                    now = now + N;
                                    break;
                                case 3:
                                    map[now - 1, 1]  = true;
                                    now = now - 1;
                                    break;
                                case 4:
                                    map[now - N, 2]  = true;
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

            //GDI engine: drawing the maze
            #region
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
            seed = -1;
        }
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
        }//to judge which way can the unit now to explore

        private void run(object sender, EventArgs e)//alg. to explore the maze
        {
            rStack.Clear();
            rStack.Push(-1);//a stupid way to solve the problem at the beginning of the 
                            //recursion but I just don't have any other way at all!
            explore(0);

            //GDI engine: drawing the key to the maze
            #region
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
            while ((next = rStack.Peek()) != -1)
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
            #endregion
        }
        private bool explore(int stackTop)
        {
            if (stackTop != N * N - 1)
            {
                int temp = 0, i;
                for (i = 1; i < 5; i++)
                {
                    if (map[stackTop, i] == true)
                    {
                        switch (i)
                        {
                            case 1:
                                temp = stackTop + 1;
                                break;
                            case 2:
                                temp = stackTop + N;
                                break;
                            case 3:
                                temp = stackTop - 1;
                                break;
                            case 4:
                                temp = stackTop - N;
                                break;
                            default: break;
                        }
                        if (temp == rStack.Peek())
                            continue;
                        else
                        {
                            rStack.Push(stackTop);
                            if (explore(temp) == true)
                                break;
                            else
                                continue;
                        }
                    }
                    else continue;
                }
                if (i == 5)
                {
                    rStack.Pop();
                    return false;
                }
            }
            else
                rStack.Push(stackTop);

            return true;
        }

        private void recovery(object sender, EventArgs e)
        {
            seed = seedLatch;
            generate(sender, e);
        }
    }
}