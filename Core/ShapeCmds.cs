using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace smartEdit.Core {
    public class CmdMoveShape : CmdBase {
        public CmdMoveShape(ShapeInterface Shape, Point OldPos, Point NewPos)
            : base(null) {
            SetContext(Shape, OldPos, NewPos);
        }
        public CmdMoveShape(ShapeInterface Shape, Point OldPos, Point NewPos
            , CmdBase Parent)
            : base(Parent) {
            SetContext(Shape, OldPos, NewPos);
        }

        public override void Undo() {
            if (m_Shape == null) return;
            m_Shape.MoveBoundingBox(m_OldPos, false);
        }
        public override void Redo() {
            if (m_Shape == null) return;
            m_Shape.MoveBoundingBox(m_NewPos, false);
        }
        private void SetContext(ShapeInterface Shape, Point OldPos, Point NewPos) {
            m_Shape = Shape;
            m_OldPos = OldPos;
            m_NewPos = NewPos;
            SetText("Move to " + NewPos.ToString());
        }
        ShapeInterface m_Shape;
        Point m_OldPos, m_NewPos;
    }
    public class CmdSizeShape : CmdBase {
        public CmdSizeShape(ShapeInterface Shape, Point OldPos, Point NewPos)
            : base(null) {
            SetContext(Shape, OldPos, NewPos);
        }
        public CmdSizeShape(ShapeInterface Shape, Point OldPos, Point NewPos
            , CmdBase Parent)
            : base(Parent) {
            SetContext(Shape, OldPos, NewPos);
        }

        public override void Undo() {
            if (m_Shape == null) return;
            m_Shape.MoveBoundingBox(m_OldPos, false);
        }
        public override void Redo() {
            if (m_Shape == null) return;
            m_Shape.MoveBoundingBox(m_NewPos, false);
        }
        private void SetContext(ShapeInterface Shape, Point OldPos, Point NewPos) {
            m_Shape = Shape;
            m_OldPos = OldPos;
            m_NewPos = NewPos;
            SetText("Size ");
        }
        ShapeInterface m_Shape;
        Point m_OldPos, m_NewPos;
    }
    public class CmdAddShape : CmdBase {
        public CmdAddShape(ModelDiagram Diagram, ShapeInterface Shape, Point PositionA, Point PositionB)
            : base(null) {
            SetContext(Diagram, Shape, PositionA, PositionB);
        }
        public CmdAddShape(ModelDiagram Diagram, ShapeInterface Shape, Point PositionA, Point PositionB,
             CmdBase Parent)
            : base(Parent) {
            SetContext(Diagram, Shape, PositionA, PositionB);
        }

        public override void Undo() {
            if (m_Shape == null || m_Diagram == null) return;
            m_Diagram.DeleteShape(m_Shape);
        }
        public override void Redo() {
            if (m_Shape == null || m_Diagram == null) return;
            m_Diagram.AddShape(m_Shape, m_PosA, m_PosB);
        }
        private void SetContext(ModelDiagram Diagram, ShapeInterface Shape, Point PositionA, Point PositionB) {
            m_Shape = Shape;
            m_PosA = PositionA;
            m_PosB = PositionB;
            m_Diagram = Diagram;
            SetText("Create " + m_Shape.GetName());
        }
        ShapeInterface m_Shape;
        Point m_PosA, m_PosB;
        ModelDiagram m_Diagram;
    }
    public class CmdDeleteShape : CmdBase {
        public CmdDeleteShape(ModelDiagram Diagram, ShapeInterface Shape)
            : base(null) {
            SetContext(Diagram, Shape);
        }
        public CmdDeleteShape(ModelDiagram Diagram, ShapeInterface Shape,
             CmdBase Parent)
            : base(Parent) {
            SetContext(Diagram, Shape);
        }

        public override void Undo() {
            if (m_Shape == null || m_Diagram == null) return;
            m_Diagram.AddShape(m_Shape);
        }
        public override void Redo() {
            if (m_Shape == null || m_Diagram == null) return;
            m_Diagram.DeleteShape(m_Shape);
        }
        private void SetContext(ModelDiagram Diagram, ShapeInterface Shape) {
            m_Shape = Shape;
            m_Diagram = Diagram;
            SetText("Delete " + m_Shape.GetName());
        }
        ShapeInterface m_Shape;
        ModelDiagram m_Diagram;
    }

}