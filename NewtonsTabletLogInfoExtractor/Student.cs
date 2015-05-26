using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewtonsTabletLogInfoExtractor
{
    class Student
    {
        public StaticsProblem[] listOfStaticsProblem;
        public string studentID;

        public Student(string studentID, int numberOfProblemWorkedOn)
        {
            this.studentID = studentID;
            this.listOfStaticsProblem = new StaticsProblem[numberOfProblemWorkedOn];
            for (int i = 0; i <= numberOfProblemWorkedOn - 1;i++ )
            {
                listOfStaticsProblem[i] = new StaticsProblem();
            }
        }
    }
}
