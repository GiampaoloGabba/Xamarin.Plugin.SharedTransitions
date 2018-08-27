using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms;

namespace Plugin.SharedTransitions
{
    /// <summary>
    /// Tag Mapper implementation
    /// </summary>
    /// <seealso cref="Plugin.SharedTransitions.ITagMapper" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TagMapper : ITagMapper
    {
        readonly Lazy<List<TagMap>> _tagStack = new Lazy<List<TagMap>>(() => new List<TagMap>());

        public IReadOnlyList<TagMap> MapStack => _tagStack.Value;

        public IReadOnlyList<Tags> GetMap(Page page, int group = 0)
        {
            return MapStack.Where(x => x.PageId == page.Id)
                           .Select(x => x.Tags.Where(y=>y.Group == group).ToList())
                           .FirstOrDefault() ?? new List<Tags>();
        }

        /// <summary>
        /// Add tags information for the specified Page to the MapStack
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="group">The group.</param>
        /// <param name="viewId">The view identifier.</param>
        public void Add(Page page, int tag, int group, int viewId)
        {
            var tagMap = _tagStack.Value.FirstOrDefault(x => x.PageId == page.Id);

            if (tagMap == null)
            {
                _tagStack.Value.Add(
                    new TagMap
                    {
                        PageId = page.Id,
                        Tags = new List<Tags>{CreateTag(tag, group, viewId) }
                    }
                );
            } else if (tagMap.Tags.All(x => x.Tag != tag))
            {
                tagMap.Tags.Add(CreateTag(tag, group, viewId));
            }
            else
            {
                //the tag exists but are we sure group and viewId are changed?
                var currentTag = tagMap.Tags.FirstOrDefault(x => x.Tag == tag);
                if (currentTag != null)
                {
                    if (currentTag.Group != group)
                        currentTag.Group = group;

                    if (currentTag.ViewId != viewId)
                        currentTag.ViewId = viewId;
                } 
            }
        }

        /// <summary>
        /// Removes the specified page from the MapStack
        /// </summary>
        /// <param name="page">The page.</param>
        public void Remove(Page page)
        {
            _tagStack.Value.Remove(_tagStack.Value.FirstOrDefault(x => x.PageId == page.Id));
        }

        /// <summary>
        /// Creates a tag with additional information
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="group">The group.</param>
        /// <param name="viewId">The view identifier.</param>
        /// <returns></returns>
        Tags CreateTag(int tag, int group, int viewId)
        {
            return new Tags
            {
                Tag    = tag,
                Group  = group,
                ViewId = viewId
            };
        }
    }
}
