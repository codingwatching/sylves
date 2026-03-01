using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sylves.Test
{
    [TestFixture]
    internal class XZModifierTest
    {
        [Test]
        public void FindCell()
        {
            var g = new XZModifier(new SquareGrid(1, new SquareBound(-10, -10, 10, 10)));
            GridTest.FindCell(g, new Cell(5, 5));
        }
    }
}
