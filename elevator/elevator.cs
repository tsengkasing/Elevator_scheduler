using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace elevator_problem
{
    public enum direction{ Null = 0, up = 1, down = 2, broken = 3 };

    public class elevator 
    {
        public int tag;                                 //电梯标示
	    public bool active;                             //电梯激活状态
	    public bool running;                            //上升或下降途中
	    public bool openButton;                         //开门键状态
	    public bool closeButton;                        //关门键状态
        public direction refuse;                        //屏蔽方向
        public int refuse_floor;                        //屏蔽楼层
	    public int now_floor;                           //当前所在楼层
        public int now_obj_floor;                       //当前目标楼层
	    public direction direct;                        //当前运行方向

	    public SortedSet<int> upQueue;                  //上升队列
        public SortedSet<int> downQueue;                //下降队列

	    public elevator(int Tag) 
        {
            tag = Tag;
            active = true;
            running = false;
            now_floor = 1;
            now_obj_floor = 0;
            refuse = direction.Null;
            refuse_floor = 0;
            direct = direction.Null;
            openButton = false;
            closeButton = false;
            upQueue = new SortedSet<int>();
            downQueue = new SortedSet<int>();
	    }                    //初始化电梯

        private void PushUpQueue(int obj_floor)         //添加到上升队列
        {
            if (upQueue.Contains(obj_floor))
            {
                return;
            }
            else
            {
                upQueue.Add(obj_floor);
            }
        }

        private void PushDownQueue(int obj_floor)       //添加到下降队列
        {
            if (downQueue.Contains(obj_floor))
            {
                return;
            }
            else
            {
                downQueue.Add(obj_floor);
            }
        }

	    public bool pressOpenButton()                   //按开门按钮
        {
		    if (!running && direct != direction.broken) {
			    openButton = true;
                return true;
		    }
            return false;
	    }

        public bool pressCloseButton()                  //按关门按钮
        {
            if (!running && direct != direction.broken)
            {
                closeButton = true;
                return true;
            }
            return false;
        }

        public bool addQueue(int obj_floor)             //添加到队列
        {
            if (direct == direction.broken)           //如果电梯已坏 不参与调度
            {
                return false;
            }
            if (now_floor == obj_floor)
            {
                pressOpenButton();
                return false;
            }
            else if (direct == direction.up)
            {
                if (obj_floor > now_obj_floor)
                {
                    PushUpQueue(obj_floor);
                }
                else
                {
                    PushDownQueue(obj_floor);
                }
            }
            else
            {
                if (obj_floor < now_obj_floor)
                {
                    PushDownQueue(obj_floor);
                }
                else
                {
                    PushUpQueue(obj_floor);
                }
            }
            return true;
        }

    };
    
}
