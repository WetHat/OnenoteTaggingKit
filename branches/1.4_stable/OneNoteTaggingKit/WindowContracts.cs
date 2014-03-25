using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using WetHatLab.OneNote.TaggingKit.find;
namespace WetHatLab.OneNote.TaggingKit
{
    /// <summary>
    /// interface for dialogs opened by this addin 
    /// </summary>
    /// <typeparam name="M"></typeparam>
    internal interface IOneNotePageWindow<M>
    {
        /// <summary>
        /// Get or set the view model backing the dialog
        /// </summary>
        M ViewModel { get; set; }
    }

    /// <summary>
    /// Contract used by the tag editor view model
    /// </summary>
    /// <seealso cref="WetHatLab.OneNote.TaggingKit.edit.TagEditor"/>
    internal interface ITagEditorModel
    {
        /// <summary>
        /// Get the collection of tags on current OneNote page.
        /// </summary>
        ObservableCollection<string> PageTags { get;}
        
        /// <summary>
        /// Get the collection of all knows tags.
        /// </summary>
        ObservableCollection<string> KnownTags { get;}
       
        /// <summary>
        /// Get the addin version.
        /// </summary>
        string AddinVersion { get;}
    }

    
    /// <summary>
    ///  Contract for view models of the <see cref="WetHatLab.OneNote.TaggingKit.manage.TagManager"/> dialog
    /// </summary>
    /// <seealso cref="WetHatLab.OneNote.TaggingKit.manage.TagManager"/>
   
    internal interface ITagManagerModel
    {
        /// <summary>
        /// Get the collection of all tags used for suggestions.
        /// </summary>
        ObservableCollection<string> SuggestedTags { get; }

        /// <summary>
        /// Get the addin version.
        /// </summary>
        string AddinVersion { get; }
    }
    /// <summary>
    /// Contract for view models of the <see cref="WetHatLab.OneNote.TaggingKit.find.FindTaggedPages"/> windows
    /// </summary>
    internal interface ITagSearchModel
    {
        /// <summary>
        /// Get a list of scopes available for finding tagged pages.
        /// </summary>
        IList<TagSearchScope> Scopes { get; }
        /// <summary>
        /// Get or set the scope to use
        /// </summary>
        TagSearchScope SelectedScope { get; set; }
        /// <summary>
        /// Fired when the collection of tags foud in a scope changes
        /// </summary>
        event NotifyCollectionChangedEventHandler TagCollectionChanged;
        /// <summary>
        /// Get the collection of pages with particular tags
        /// </summary>
        ObservableCollection<HitHighlightedPage> Pages { get; }
    }
}
