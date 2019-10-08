
//using NUnit.Framework;
//using System.Collections.Generic;

//namespace Saro.BT
//{
//    public class VariableTest
//    {
//        const int N = 1000000;
//        LinkedList<int> ll;
//        List<int> l;

//        [SetUp]
//        public void SetUp()
//        {
//            ll = new LinkedList<int>();
//            l = new List<int>(N);
//            for (int i = 0; i < N; i++)
//            {
//                ll.AddLast(i);
//                l.Add(i);
//            }
//        }

//        [Test]
//        public void List()
//        {
//            for (int i = 0; i < N; i++)
//            {
//                l[i].ToString();
//            }
//        }

//        [Test]
//        public void List_Foreach()
//        {
//            foreach (var i in l)
//            {
//                i.ToString();
//            }
//        }

//        [Test]
//        public void LinkedList_Foreach()
//        {
//            foreach (var node in ll)
//            {
//                node.ToString();
//            }
//        }

//        [Test]
//        public void LinkedList_While()
//        {
//            var head = ll.First;
//            while (head != null)
//            {
//                head.Value.ToString();
//                head = head.Next;
//            }
//        }
//    }
//}