using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Core
{
    public class RectPackNode
    {
        public Rect Bounds { get; private set; }
        public float Area => Bounds.size.x * Bounds.size.y;

        public bool HasContent { get; private set; }
        public Rect Content { get; private set; }

        public RectPackNode ChildA { get; private set; }
        public RectPackNode ChildB { get; private set; }

        public RectPackNode(Rect bounds)
        {
            this.Bounds = bounds;
        }

        public RectPackNode(in float2 size)
        {
            this.Bounds = new Rect(float2.Zero, size);
        }

        public bool Fits(Rect rect) => Fits(rect.size);
        public bool Fits(in float2 size) => size.x <= Bounds.width && size.y <= Bounds.height;
        public bool Fits(float area) => Bounds.width * Bounds.height >= area;

        public void Pack(in float2 size)
        {
            if (HasContent)
                throw new Exception("This node already has rect stored inside");

            if (!Fits(size))
                throw new Exception("Rect is too large for this node!");

            // set the rect position to the top left corner of the bounds
            Content = new Rect(Bounds.position, size);
            HasContent = true;

            // Create two empty children
            ChildA = new RectPackNode(new Rect(Content.xmax, Content.y, Bounds.width-Content.width, Content.height));
            ChildB = new RectPackNode(new Rect(Content.x, Content.ymax, Bounds.width, Bounds.height - Content.height));
        }

        public RectPackNode ExtendHorizontal(float extraSize)
        {
            var rootBounds = this.Bounds;
            rootBounds.width += extraSize;

            var newRoot = new RectPackNode(rootBounds);

            newRoot.HasContent = true;
            newRoot.Content = new Rect(rootBounds.x, rootBounds.y, 0, 0);

            newRoot.ChildA = this;
            newRoot.ChildB = new RectPackNode(new Rect(Bounds.x + Bounds.width, Bounds.y, extraSize, Bounds.height));

            return newRoot;
        }

        public RectPackNode ExtendVertical(float extraSize)
        {
            var rootBounds = this.Bounds;
            rootBounds.height += extraSize;

            var newRoot = new RectPackNode(rootBounds);

            newRoot.HasContent = true;
            newRoot.Content = new Rect(rootBounds.x, rootBounds.y, 0, 0);

            newRoot.ChildA = this;
            newRoot.ChildB = new RectPackNode(new Rect(Bounds.x, Bounds.y + Bounds.height, Bounds.width, extraSize));

            return newRoot;
        }

        public void Partition(float maxSize)
        {
            if (HasContent)
                return;

            if(Bounds.width > maxSize)
            {
                HasContent = true;
                Content = new Rect(Bounds.position, float2.Zero);

                var remainder = Bounds.width - maxSize;

                ChildA = new RectPackNode(new Rect(Bounds.x, Bounds.y, maxSize, Bounds.height));
                ChildB = new RectPackNode(new Rect(Bounds.x + maxSize, Bounds.y, remainder, Bounds.height));

                ChildA.Partition(maxSize);
                ChildB.Partition(maxSize);
            }
            else if(Bounds.height > maxSize)
            {
                HasContent = true;
                Content = new Rect(Bounds.position, float2.Zero);

                var remainder = Bounds.height - maxSize;

                ChildA = new RectPackNode(new Rect(Bounds.x, Bounds.y, Bounds.width, maxSize));
                ChildB = new RectPackNode(new Rect(Bounds.x, Bounds.y + maxSize, Bounds.width, remainder));

                ChildA.Partition(maxSize);
                ChildB.Partition(maxSize);
            }
        }

        public RectPackNode GetEmptyNode(in float2 size)
        {
            if (!Fits(size))
                return null;

            if (HasContent)
                return ChildA.GetEmptyNode(size) ?? ChildB.GetEmptyNode(size);
            else if (Fits(size))
                return this;

            // nothing found
            return null;
        }

        public RectPackNode GetEmptyNode(float area)
        {
            if (!Fits(area))
                return null;

            if (HasContent)
                return ChildA.GetEmptyNode(area) ?? ChildB.GetEmptyNode(area);
            else if (Fits(area))
                return this;

            return null;
        }

        public RectPackNode GetEmptyNode(float minArea, float maxArea)
        {
            if (!Fits(minArea))
                return null;

            if (HasContent)
            {
                var a = ChildA.GetEmptyNode(minArea, maxArea);
                
                if(a != null)
                {
                    // stop search, the node satisfies the max area
                    var area = a.Bounds.size.x * a.Bounds.size.y;
                    if (area >= maxArea)
                        return a;
                }

                var b = ChildB.GetEmptyNode(minArea, maxArea);

                if (a != null && b != null)
                    return (a.Area > b.Area) ? a : b;

                return a ?? b;
            }
            else if (Fits(minArea))
                return this;

            return null;
        }

        public void ExpandAll()
        {
            if (HasContent)
            {
                // Only expand if the child is free
                if (ChildA.HasContent)
                    ChildA.ExpandAll();
                else
                {
                    Content = new Rect(Content.position, new float2(Bounds.width, Content.height));
                    ChildA.Bounds = new Rect(ChildA.Bounds.position, float2.Zero);
                }

                if (ChildB.HasContent)
                    ChildB.ExpandAll();
                else
                {
                    Content = new Rect(Content.position, new float2(Content.width, Bounds.height));
                    ChildB.Bounds = new Rect(ChildB.Bounds.position + new float2(Content.width),
                        ChildB.Bounds.size - new float2(Content.width, 0f));
                }
            }
        }

        public float ComputeTotalFreeArea()
        {
            if (HasContent)
                return ChildA.ComputeTotalFreeArea() + ChildB.ComputeTotalFreeArea();
            else
                return Bounds.size.x * Bounds.size.y;
        }

        public float ComputeLargestFreeArea()
        {
            if (HasContent)
                return MathX.Max(ChildA.ComputeLargestFreeArea(), ChildB.ComputeLargestFreeArea());
            else
                return Bounds.size.x * Bounds.size.y;
        }
    }
}
