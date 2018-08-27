using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Plugin.SharedTransitions
{
    /// <summary>
    /// Interface for TagMapper 
    /// </summary>
    public interface ITagMapper
    {
        IReadOnlyList<TagMap> MapStack { get; }
        IReadOnlyList<Tags> GetMap(Page page, int group = 0);
        void Add(Page page, int tag, int group, int viewId);
        void Remove(Page page);
    }


    /// <summary>
    /// TagMap used to associate pages with their tags 
    /// </summary>
    public class TagMap
    {
        public Guid PageId { get; set; }
        public List<Tags> Tags { get; set; }
    }

    /// <summary>
    /// Tags details
    /// </summary>
    public class Tags
    {
        public int Tag { get; set; }
        public int Group { get; set; }
        public int ViewId { get; set; }
    }
}
