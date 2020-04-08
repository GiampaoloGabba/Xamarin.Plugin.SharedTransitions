using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Plugin.SharedTransitions
{
    /// <summary>
    /// Interface for TransitionMapper 
    /// </summary>
    public interface ITransitionMapper
    {
        /// <summary>
        /// Transition stack containing all the registered shared transitions
        /// </summary>
        IReadOnlyList<TransitionMap> TransitionStack { get; }

        /// <summary>
        /// Get the transition stack associated to a page
        /// </summary>
        /// <param name="page">Page to check</param>
        /// <param name="selectedGroup">Group to filter, if set to null will search for Group=null, dont use this to ignore group filter</param>
        /// /// <param name="ignoreGroup">True to completely ignore the filter group</param>
        IReadOnlyList<TransitionDetail> GetMap(Page page, string selectedGroup, bool ignoreGroup = false);

        /// <summary>
        /// Add transition information for the specified Page to the TransitionStack
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="transitionName">The name of the shared transition.</param>
        /// <param name="transitionGroup">The transition group for dynamic transitions.</param>
        /// <param name="view">The Xamarin Forms view associated to this transition.</param>
        /// <param name="nativeViewId">The Native view unique identifier.</param>
        /// <param name="nativeView">The Native View</param>
        int AddOrUpdate(Page page, string transitionName, string transitionGroup, View view, int nativeViewId, object nativeView);

        /// <summary>
        /// Clear the MapStack from old reference for recycled elements
        /// </summary>
        /// <param name="transitionMap">The transition map to clear</param>
        /// <param name="transitionDetail">The new transitiondetail to check for old, recycled, elements</param>
        void ClearMapStackForElementRecycling(TransitionMap transitionMap, TransitionDetail transitionDetail);

        /// <summary>
        /// Removes the specified transitionDetail from the TransitionStack
        /// </summary>
        /// <param name="view">The Xamarin Forms view associated to remove from transition.</param>
        /// <param name="page">The page.</param>
        void Remove(Page page, View view);

        /// <summary>
        /// Removes the specified transitionDetail from the TransitionStack
        /// </summary>
        /// <param name="nativeViewId">The Native view unique identifier.</param>
        /// <param name="page">The page.</param>
        void Remove(Page page, int nativeViewId);

        /// <summary>
        /// Removes the specified page from the TransitionStack
        /// </summary>
        /// <param name="page">The page.</param>
        void RemoveFromPage(Page page);

        /// <summary>
        /// Creates a transition with additional information
        /// </summary>
        /// <param name="transitionName">The name of the shared transition.</param>
        /// <param name="transitionGroup">The transition group for dynamic transitions.</param>
        /// <param name="view">The Xamarin Forms view associated to this transition.</param>
        /// <param name="nativeViewId">The Native view unique identifier.</param>
        /// <param name="nativeView">The Native View</param>
        TransitionDetail CreateTransition(string transitionName, string transitionGroup, View view, int nativeViewId, object nativeView);
    }

    /// <summary>
    /// TransitionMap used to associate pages with their transitions 
    /// </summary>
    public class TransitionMap
    {
        /// <summary>
        /// Page with the associated transitions
        /// </summary>
        public Page Page { get; set; }
        /// <summary>
        /// List of associated transitions to the Page
        /// </summary>
        public List<TransitionDetail> Transitions { get; set; }
    }

    /// <summary>
    /// Transition detail
    /// </summary>
    public class TransitionDetail
    {
        /// <summary>
        /// Transition name. The combination of Name and Group must be unique per page
        /// </summary>
        public string TransitionName { get; set; }
        
        /// <summary>
        /// Transition Group for dynamic transitions
        /// </summary>
        public string TransitionGroup { get; set; }

        /// <summary>
        /// Xamarin Forms View associated with the transitions
        /// </summary>
        public View View { get; set; }

        /// <summary>
        /// Native View Id (or Tag for iOS) associated with the transitions
        /// </summary>
        public int NativeViewId { get; set; }

        public object NativeView { get; set; }

        /// <summary>
        /// Dirty flag, if true this transition will be cleaned at the next mapping check for the page
        /// </summary>
        public bool IsDirty { get; set; }
    }
}