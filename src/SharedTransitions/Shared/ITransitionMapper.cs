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
        IReadOnlyList<TransitionMap> TransitionStack { get; }
        IReadOnlyList<TransitionDetail> GetMap(Page page, string selectedGroup = null);
        int Add(Page page, string transitionName, string transitionGroup, Guid formsViewId, int nativeViewId);
        void Remove(Page page);
    }

    /// <summary>
    /// TransitionMap used to associate pages with their transitions 
    /// </summary>
    public class TransitionMap
    {
        public Guid PageId { get; set; }
        public List<TransitionDetail> Transitions { get; set; }
    }

    /// <summary>
    /// Transition detail
    /// </summary>
    public class TransitionDetail
    {
        public string TransitionName { get; set; }
        public string TransitionGroup { get; set; }
        public Guid FormsViewId { get; set; }
        public int NativeViewId { get; set; }
    }
}