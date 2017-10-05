using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace resultys.prospecta.lib
{
    public class Semaphore
    {
        public int capacity { get; set; }

        public string name { get; set; }
        private bool locked { get; set; }
        private int totalRelease;

        public Semaphore()
        {
            this.locked = false;
        }

        public Semaphore(string name) : this()
        {
            this.name = name;
        }

        public Semaphore(int capacity) : this()
        {
            this.capacity = capacity;            
        }

        public Semaphore(string name, int capacity): this(name)
        {
            this.capacity = capacity;
        }

        public void release()
        {
            this.totalRelease++;

            if (this.totalRelease >= this.capacity) this.locked = false;
        }

        public void wait()
        {
            if (this.totalRelease >= this.capacity) return;
            if (this.capacity == 0) return;

            this.locked = true;

            while (this.locked) System.Threading.Thread.Sleep(100);
        }

        public void reset()
        {
            this.totalRelease = 0;
        }

    }
}
