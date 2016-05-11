using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace elevator_problem
{
    public partial class Elevator_status : Form
    {
        bool Active;                                                //程序激活状态
        int select_to_fix = 0;                                      //选择修复的电梯

        const int MIN_WAIT_TIME = 100;                              //等待时间最小单位

        bool[] buttonUpOn;                                          //上行键按钮
        bool[] buttonDownOn;                                        //下行键按钮

        List<Thread> threadlift = new List<Thread>();               //存储线程的数组
        elevator[] ele = new elevator[5];                           //存储电梯实例的数组
        SortedDictionary<int, int> eleSelect = new SortedDictionary<int, int>();    //调度电梯距离排序

        /* 颜色资源加载 */
        Color lightGreen = Color.FromArgb(19, 224, 234);

        /* 图片资源加载 */
        Bitmap emptyIcon = new Bitmap("./Resources/elevator_null.png");
        Bitmap freeIcon = new Bitmap("./Resources/elevator_free.png");
        Bitmap movingIcon = new Bitmap("./Resources/elevator_run.png");
        Bitmap openIcon = new Bitmap("./Resources/elevator_open.png");
        Bitmap brokenIcon = new Bitmap("./Resources/elevator_broken.png");

        public Elevator_status()                                        //界面初始化
        {
            InitializeComponent();
            InitialButton();
            InitialFontAndImage();

            elevator_frame.RowHeadersWidth = 60;
            elevator_frame.ColumnHeadersHeight = 60;
            elevator_frame.AllowUserToAddRows = false;

            int floor = 0;
            for (int i = 20; i > 0; i--)
            {
                floor = elevator_frame.Rows.Add();
                elevator_frame.Rows[floor].HeaderCell.Value = i.ToString();
                elevator_frame.Rows[floor].Height = 34;
            }
            elevator_frame.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);

            Begin();
        }

        public void InitialButton()                                     //按钮初始化
        {
            buttonUpOn = new bool[19];
            buttonDownOn = new bool[19]; 

            //上行下行键
            this.buttonUp = new PictureBox[19];
            this.buttonDown = new PictureBox[19];
            for (int i = 1; i < 20; i++)
            {
                this.buttonUp[i - 1] = (PictureBox)(this.Controls.Find("buttonUp_" + i.ToString(), true)[0]);
                this.buttonDown[i - 1] = (PictureBox)(this.Controls.Find("buttonDown_" + (i + 1).ToString(), true)[0]);

                buttonUpOn[i - 1] = false;
                buttonDownOn[i - 1] = false;
                this.buttonUp[i - 1].Image = new Bitmap("./Resources/buttonUp.png");
                this.buttonDown[i - 1].Image = new Bitmap("./Resources/buttonDown.png");
            }

            //开门关门报警键
            this.buttonOpen = new PictureBox[5];
            this.buttonClose = new PictureBox[5];
            this.buttonWarning = new PictureBox[5];
            for (int i = 1; i < 6; i++)
            {
                this.buttonOpen[i - 1] = (PictureBox)(this.Controls.Find("elevator_" + i.ToString() + "_buttonOpen", true)[0]);
                this.buttonClose[i - 1] = (PictureBox)(this.Controls.Find("elevator_" + i.ToString() + "_buttonClose", true)[0]);
                this.buttonWarning[i - 1] = (PictureBox)(this.Controls.Find("elevator_" + i.ToString() + "_buttonWarning", true)[0]);

                this.buttonOpen[i - 1].Image = new Bitmap("./Resources/buttonOpen.png");
                this.buttonClose[i - 1].Image = new Bitmap("./Resources/buttonClose.png");
                this.buttonWarning[i - 1].Image = new Bitmap("./Resources/buttonWarn.png");
            }

            //数字键
            this.elevator_Floor = new Label[5][];
            for (int i = 0; i < 5; i++)
            {
                this.elevator_Floor[i] = new Label[20];
                for (int j = 1; j <= 20; j++)
                {
                    elevator_Floor[i][j - 1] = (Label)(this.Controls.Find("elevator_" + (i + 1).ToString() + "_" + j.ToString(), true)[0]);
                }
            }

            //数码显示
            this.elevator_label = new Label[5];
            for (int i = 0; i < 5; i++)
            {
                this.elevator_label[i] = (Label)(this.Controls.Find("elevator_label_" + (i + 1).ToString(), true)[0]);
            }
        }

        public void InitialFontAndImage()
        {
            this.selfDefineFont = new System.Drawing.Text.PrivateFontCollection();
            this.selfDefineFont.AddFontFile("./Resources/MFShangHei_Noncommercial-Regular.otf");
            this.selfDefineFont.AddFontFile("./Resources/MFShangYa_Noncommercial-Regular.otf");
            this.shanghei = selfDefineFont.Families[1];
            this.shangya = selfDefineFont.Families[0];

            this.App_label.Font = new System.Drawing.Font(this.shanghei, 39.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.select_elevator_fix.Font = new System.Drawing.Font(this.shangya, 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_Fix.Font = new System.Drawing.Font(this.shanghei, 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_fix.Font = new System.Drawing.Font(this.shangya, 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.StudentNumber.Font = new System.Drawing.Font(this.shanghei, 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.StudentName.Font = new System.Drawing.Font(this.shangya, 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Mailbox.Font = new System.Drawing.Font(this.shanghei, 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.elevator_frame.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font(this.shanghei, 9.749999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.elevator_frame.RowHeadersDefaultCellStyle.Font = new System.Drawing.Font(this.shanghei, 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));

            this.elevator_label_1.Font = new System.Drawing.Font(this.shanghei, 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.elevator_1_labelname.Font = new System.Drawing.Font(this.shanghei, 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.elevator_label_2.Font = new System.Drawing.Font(this.shanghei, 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.elevator_2_labelname.Font = new System.Drawing.Font(this.shanghei, 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.elevator_label_3.Font = new System.Drawing.Font(this.shanghei, 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.elevator_3_labelname.Font = new System.Drawing.Font(this.shanghei, 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.elevator_label_4.Font = new System.Drawing.Font(this.shanghei, 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.elevator_4_labelname.Font = new System.Drawing.Font(this.shanghei, 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.elevator_label_5.Font = new System.Drawing.Font(this.shanghei, 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.elevator_5_labelname.Font = new System.Drawing.Font(this.shanghei, 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            
            this.BackgroundImage = new System.Drawing.Bitmap("./Resources/background.jpg");
            this.Icon = new System.Drawing.Icon("./Resources/icon.ico");
        }

        public void run(ref elevator e)                                 //电梯调度
        {
            int dist_floor = 0, wait = 0;
            while (e.active)//电梯工作时
            {
                
                if (e.direct == direction.up)//如果此时向上运行
                {
                    e.running = true;
                    if (e.upQueue.Count == 0)//空队列 则停留
                    {
                        if (e.downQueue.Count != 0)
                        {
                            e.direct = direction.down;
                            e.refuse = direction.Null;
                            continue;
                        }
                        e.direct = direction.Null;
                        e.refuse = direction.Null;
                        continue;
                    }
                    else
                    {
                        e.now_obj_floor = e.upQueue.Min;					//当前目标层
                        e.upQueue.Remove(e.now_obj_floor);
                        dist_floor = e.now_obj_floor - e.now_floor;			//算出距离
                        if(e.now_obj_floor < 1 || e.now_obj_floor > 20 || dist_floor < 0)
                        {
                            continue;
                        }
                        dist_floor--;								//电梯从零加速上升一层需要两秒
                        move(e.tag, e.now_floor, e.now_floor + 1);
                        e.now_floor++;
                        SetNumeric(e.tag, e.now_floor, direction.up);
                        Thread.Sleep(MIN_WAIT_TIME * 20);
                        while (dist_floor > 0)
                        {
                            move(e.tag, e.now_floor, e.now_floor + 1);
                            e.now_floor++;
                            SetNumeric(e.tag, e.now_floor ,direction.up);
                            dist_floor--;							//上升一层后，每一层需要一秒
                            Thread.Sleep(10 * MIN_WAIT_TIME);
                        }
                        e.running = false;
                        SetIcon(e.tag, e.now_floor, 2);
                        if (e.now_floor != 20)    //熄灭上行键灯
                        {
                            this.buttonUp[e.now_floor - 1].BackColor = Color.Transparent;
                            buttonUpOn[e.now_floor - 1] = false;
                        }
                        this.elevator_Floor[e.tag][e.now_floor - 1].ForeColor = lightGreen;
                        wait = 8;									    //开门停留八秒
                        while (wait != 0)
                        {
                            wait--;
                            Thread.Sleep(10 * MIN_WAIT_TIME);
                            if (e.openButton)
                            {						                    //按了开门键多停两秒
                                wait = wait + 2;
                                e.openButton = false;
                                buttonOpen[e.tag].BackColor = Color.Transparent;
                            }
                            if (e.closeButton)
                            {					                        //按了关门键马上关门
                                wait = 0;
                                e.closeButton = false;
                                buttonClose[e.tag].BackColor = Color.Transparent;
                                continue;
                            }
                        }
                        //熄灭下行键灯
                        this.buttonDown[e.now_floor - 2].BackColor = Color.Transparent;
                        buttonDownOn[e.now_floor - 2] = false;
                        SetIcon(e.tag, e.now_floor, 1);
                        e.running = true;
                    }
                }
                else if (e.direct == direction.down)                    //如果此时向下运行
                {
                    e.running = true;
                    if (e.downQueue.Count == 0)
                    {
                        if (e.upQueue.Count != 0)
                        {
                            e.direct = direction.up;
                            e.refuse = direction.Null;
                            continue;
                        }
                        e.direct = direction.Null;
                        e.refuse = direction.Null;
                        continue;
                    }
                    else
                    {
                        e.now_obj_floor = e.downQueue.Max;
                        e.downQueue.Remove(e.now_obj_floor);
                        dist_floor = e.now_floor - e.now_obj_floor;			//算出距离
                        if (e.now_obj_floor < 1 || e.now_obj_floor > 20 || dist_floor < 0)
                        {
                            continue;
                        }
                        dist_floor--;								    
                        move(e.tag, e.now_floor, e.now_floor - 1);
                        e.now_floor--;
                        SetNumeric(e.tag, e.now_floor, direction.down);
                        Thread.Sleep(MIN_WAIT_TIME * 20);                //电梯从零加速下降一层需要两秒
                        while (dist_floor > 0)
                        {
                            move(e.tag, e.now_floor, e.now_floor - 1);
                            e.now_floor--;
                            SetNumeric(e.tag, e.now_floor, direction.down);
                            dist_floor--;							    //下降一层后，每一层需要一秒
                            Thread.Sleep(10 * MIN_WAIT_TIME);
                        }
                        e.running = false;
                        SetIcon(e.tag, e.now_floor, 2);
                        if (e.now_floor != 1) //熄灭下行键灯
                        {
                            this.buttonDown[e.now_floor - 2].BackColor = Color.Transparent;
                            buttonDownOn[e.now_floor - 2] = false;
                        }
                        this.elevator_Floor[e.tag][e.now_floor - 1].ForeColor = lightGreen;
                        wait = 8;									       //开门停留八秒
                        while (wait != 0)
                        {
                            wait--;
                            Thread.Sleep(10 * MIN_WAIT_TIME);
                            if (e.openButton)
                            {						                        //按了开门键多停两秒
                                wait = wait + 2;
                                e.openButton = false;
                                buttonOpen[e.tag].BackColor = Color.Transparent;
                            }
                            if (e.closeButton)
                            {					                            //按了关门键马上关门
                                wait = 0;
                                e.closeButton = false;
                                buttonClose[e.tag].BackColor = Color.Transparent;
                                continue;
                            }
                        }
                        //熄灭上行键灯
                        this.buttonUp[e.now_floor - 1].BackColor = Color.Transparent;
                        buttonUpOn[e.now_floor - 1] = false;
                        SetIcon(e.tag, e.now_floor, 1);
                        e.running = true;
                    }
                }
                else if(e.direct == direction.Null)                          //切换到空闲状态
                {
                    SetNumeric(e.tag, e.now_floor, direction.Null);
                    SetIcon(e.tag, e.now_floor, 0);
                    e.refuse = direction.Null;
                    e.running = false;
                    if (e.openButton)
                    {
                        SetIcon(e.tag, e.now_floor, 2);
                        Thread.Sleep(MIN_WAIT_TIME * 20);
                        e.openButton = false;
                        SetIcon(e.tag, e.now_floor, 0);
                        buttonOpen[e.tag].BackColor = Color.Transparent;
                    }
                    if(e.closeButton)
                    {
                        e.closeButton = false;
                        buttonClose[e.tag].BackColor = Color.Transparent;
                    }
                    if (e.upQueue.Count > 0)
                    {
                        e.direct = direction.up;
                    }
                    else if (e.downQueue.Count > 0)
                    {
                        e.direct = direction.down;
                    }
                    Thread.Sleep(MIN_WAIT_TIME);									//停留
                    continue;
                }
                else            //电梯已坏 等待修复
                {
                    Thread.Sleep(MIN_WAIT_TIME * 2);
                }
            }
        }

        public bool people_wait(int wait_floor, direction wait_direct)         //外部等待
        {
            int dist = 0, temp = 0;
            if(!Active)
            {
                return false;
            }
	        eleSelect.Clear();
	        for (int i = 0; i < 5; i++) {
                Thread.Sleep(MIN_WAIT_TIME);
                if(ele[i].direct == direction.broken)           //如果电梯已坏 不参与调度
                {
                    continue;
                }
                //同层楼且方向相同或电梯空闲
                if (ele[i].now_floor == wait_floor && (ele[i].direct == wait_direct || ele[i].direct == direction.Null))
                {
                    ele[i].pressOpenButton();
                    if(ele[i].now_floor < 20)
                        buttonUpOn[ele[i].now_floor - 1] = false;
                    if(ele[i].now_floor > 1)
                        buttonDownOn[ele[i].now_floor - 2] = false;
                    return false;
		        }
                else         //不同层楼
                {
                    if (ele[i].direct == direction.Null)    //电梯空闲
                    {
                        dist = Math.Abs(ele[i].now_floor - wait_floor);
                    }
                    else if (ele[i].direct == wait_direct) //方向相同
                    {
                        //如果去接一个下行的人 不再去此目标层以上的楼层 同理 去接一个上行的人 不再去此目标楼层以下的楼层
                        if ((ele[i].refuse == direction.up && ele[i].refuse_floor < wait_floor) || (ele[i].refuse == direction.down && ele[i].refuse_floor > wait_floor))
                        {
                            continue;
                        }
                        if ((ele[i].now_floor < wait_floor && wait_direct == direction.up) || (ele[i].now_floor > wait_floor && wait_direct == direction.down))
                        {
                            dist = Math.Abs(ele[i].now_floor - wait_floor);
                        }
                        else
                        {
                            if (wait_direct == direction.up)
                            {
                                if (ele[i].upQueue.Count == 0)
                                {
                                    dist = ele[i].now_floor - wait_floor;
                                }
                                else
                                {
                                    temp = ele[i].upQueue.Max();
                                    dist = temp - ele[i].now_floor + temp - wait_floor;
                                }
                            }
                            else
                            {
                                if (ele[i].downQueue.Count == 0)
                                {
                                    dist = wait_floor - ele[i].now_floor;
                                }
                                else
                                {
                                    temp = ele[i].downQueue.Min();
                                    dist = ele[i].now_floor - temp + wait_floor - temp;
                                }
                            }
                        }
                    }
                    else  //方向不同
                    {
                        continue;
                    }
                    //加入到选择队列中
                    if (!eleSelect.ContainsKey(dist))
                    {
                        eleSelect.Add(dist, i);
                    }
                }
	        }
            if(eleSelect.Count == 0)
            {
                return false;
            }
            int select = eleSelect.First().Value;
            if(wait_direct == direction.up)
            {
                ele[select].refuse = direction.down;
            }
            else if(wait_direct == direction.down)
            {
                ele[select].refuse = direction.up;
            }
            ele[select].refuse_floor = wait_floor;
            ele[select].addQueue(wait_floor);

            
           
            return true;
        }

        delegate void SetNumericCallBack(int ele, int floor, direction direct);

        public void SetNumeric(int ele, int floor, direction direct)          //设置数码显示楼层
        {
            String Direct;
            switch (direct)
            {
                case direction.up:
                    Direct = " ↑";break;
                case direction.down:
                    Direct = " ↓";break;
                default:
                    Direct = "";break;
            }
            if(elevator_label[ele].InvokeRequired)
            {
                SetNumericCallBack set = new SetNumericCallBack(SetNumeric);
                elevator_label[ele].Invoke(set, new object[] { ele, floor , direct });
            }
            else
            {
                if(floor != 0)
                {
                    elevator_label[ele].Text = floor.ToString() + Direct;
                }
                else
                {
                    elevator_label[ele].Text = "X";
                    elevator_label[ele].ForeColor = Color.Red;
                }
            }
        }

        public void SetIcon(int ele, int target, int icon)              //设置方块
        {
            switch (icon)
            {
                case 0:
                    elevator_frame[ele, 20 - target].Value = freeIcon;
                    break;
                case 1:
                    elevator_frame[ele, 20 - target].Value = movingIcon;
                    break;
                case 2:
                    elevator_frame[ele, 20 - target].Value = openIcon;
                    break;
                case 3:
                    elevator_frame[ele, 20 - target].Value = emptyIcon;
                    break;
                case 4:
                    elevator_frame[ele, 20 - target].Value = brokenIcon;
                    break;
            }            
        }

        public void move(int ele, int from, int to)                     //电梯移动
        {
            if(to > 20 || to < 1)
            {
                return;
            }
            elevator_frame[ele, 20 - from].Value = emptyIcon;
            elevator_frame[ele, 20 - to].Value = movingIcon;
        }


        private void Begin()                                            //开始
        {
            Active = true;
            for (int i = 0; i < 5; i++)
            {
                int x = i;
                ele[i] = new elevator(i);
                threadlift.Add(new Thread(delegate() { run(ref ele[x]); }));
                threadlift[i].Start();
                elevator_frame[i, 19].Value = freeIcon;
            }
        }

        private void buttonUp_Click(object sender, EventArgs e)                     //上行键
        {
            PictureBox button = (PictureBox)sender;
            int floor = Int32.Parse(button.Tag.ToString());
            if(buttonUpOn[floor - 1])
            {
                return;
            }
            buttonUpOn[floor - 1] = true;
            if (people_wait(floor, direction.up))
            {
                button.BackColor = Color.Red;
            }
            
        }

        private void buttonDown_Click(object sender, EventArgs e)                   //下行键
        {
            PictureBox button = (PictureBox)sender;
            int floor = Int32.Parse(button.Tag.ToString());
            if (buttonDownOn[floor - 2])
            {
                return;
            }
            buttonDownOn[floor - 2] = true;
            if(people_wait(floor, direction.down))
            {
                button.BackColor = Color.Red;
            }
        }

        private void buttonOpen_Click(object sender, EventArgs e)                   //开门键
        {
            PictureBox button = (PictureBox)sender;
            int lift = Int32.Parse(button.Tag.ToString()) - 1;
            int floor = ele[lift].now_floor;
            if(ele[lift].pressOpenButton())
            {
                if(floor < 20)
                    buttonUpOn[floor - 1] = false;
                if(floor > 1)
                    buttonDownOn[floor - 2] = false;
                button.BackColor = Color.Red;
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)                  //关门键
        {
            PictureBox button = (PictureBox)sender;
            if (ele[Int32.Parse(button.Tag.ToString()) - 1].pressCloseButton())
            {
                button.BackColor = Color.Red;
            }
        }

        private void buttonWarning_Click(object sender, EventArgs e)                //报警键
        {
            PictureBox button = (PictureBox)sender;
            int pos = Int32.Parse(button.Tag.ToString());
            ele[pos - 1].direct = direction.broken;
            ele[pos - 1].upQueue.Clear();
            ele[pos - 1].downQueue.Clear();
            threadlift[pos - 1].Suspend();
            SetNumeric(pos - 1, 0, direction.broken);
            SetIcon(pos - 1, ele[pos - 1].now_floor, 4);
        }

        private void elevator_1_Floor_Click(object sender, EventArgs e)             //电梯1数字键
        {
            Label label = (Label)sender;
            if (ele[0].addQueue(Int32.Parse(label.Tag.ToString())))
            {
                label.ForeColor = Color.Red;
            }
        }

        private void elevator_2_Floor_Click(object sender, EventArgs e)             //电梯2数字键
        {
            Label label = (Label)sender;
            if (ele[1].addQueue(Int32.Parse(label.Tag.ToString())))
            {
                label.ForeColor = Color.Red;
            }
        }

        private void elevator_3_Floor_Click(object sender, EventArgs e)             //电梯3数字键
        {
            Label label = (Label)sender;
            if (ele[2].addQueue(Int32.Parse(label.Tag.ToString())))
            {
                label.ForeColor = Color.Red;
            }
        }

        private void elevator_4_Floor_Click(object sender, EventArgs e)             //电梯4数字键
        {
            Label label = (Label)sender;
            if (ele[3].addQueue(Int32.Parse(label.Tag.ToString())))
            {
                label.ForeColor = Color.Red;
            }
        }

        private void elevator_5_Floor_Click(object sender, EventArgs e)             //电梯5数字键
        {
            Label label = (Label)sender;
            if (ele[4].addQueue(Int32.Parse(label.Tag.ToString())))
            {
                label.ForeColor = Color.Red;
            }
        }

        private void select_elevator_fix_SelectedIndexChanged(object sender, EventArgs e)   //选择修复的电梯
        {
            ComboBox box = (ComboBox)sender;
            select_to_fix = box.SelectedIndex;
        }

        private void button_Fix_Click(object sender, EventArgs e)                           //修复电梯
        {
            if(ele[select_to_fix].direct == direction.broken)
            {
                ele[select_to_fix].direct = direction.Null;
                ele[select_to_fix].refuse = direction.Null;
                threadlift[select_to_fix].Resume();
                SetIcon(select_to_fix, ele[select_to_fix].now_floor, 0);
                elevator_label[select_to_fix].ForeColor = lightGreen;
                elevator_label[select_to_fix].Text = ele[select_to_fix].now_floor.ToString();
            }
        }

        private void Elevator_status_Closing(object sender, FormClosingEventArgs e)         //关闭线程
        {
            Active = false;
            for (int i = 0; i < 5; i++)
            {
                ele[i].active = false;
                if(threadlift[i].ThreadState == ThreadState.Suspended)
                {
                    threadlift[i].Resume();
                }
                threadlift[i].Abort();
            }
        }
    }
}
