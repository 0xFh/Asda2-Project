using System;
using WCell.Util.Graphics;
using WCell.Util.Threading;

namespace WCell.Core.Paths
{
    public class PathQuery
    {
        private Vector3 from;
        private Vector3 to;
        private readonly IContextHandler m_ContextHandler;
        private PathQuery.PathQueryCallback callback;

        public PathQuery(Vector3 from, ref Vector3 to, IContextHandler contextHandler,
            PathQuery.PathQueryCallback callback)
        {
            this.from = from;
            this.to = to;
            this.m_ContextHandler = contextHandler;
            this.callback = callback;
            this.Path = new Path();
        }

        public PathQuery(Vector3 from, Vector3 to, IContextHandler contextHandler, PathQuery.PathQueryCallback callback)
        {
            this.from = from;
            this.to = to;
            this.m_ContextHandler = contextHandler;
            this.callback = callback;
            this.Path = new Path();
        }

        public Vector3 From
        {
            get { return this.from; }
        }

        public Vector3 To
        {
            get { return this.to; }
        }

        public IContextHandler ContextHandler
        {
            get { return this.m_ContextHandler; }
        }

        public PathQuery.PathQueryCallback Callback
        {
            get { return this.callback; }
        }

        public Path Path { get; private set; }

        public void Reply()
        {
            if (this.m_ContextHandler != null)
                this.m_ContextHandler.ExecuteInContext((Action) (() => this.callback(this)));
            else
                this.callback(this);
        }

        public delegate void PathQueryCallback(PathQuery query);
    }
}