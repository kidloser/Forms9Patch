using System;
using Xamarin.Forms;
using UIKit;
using Foundation;
using MobileCoreServices;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.Platform.iOS;
using System.IO;
using System.Diagnostics;
using ObjCRuntime;
using System.Threading.Tasks;

[assembly: Dependency(typeof(Forms9Patch.iOS.ClipboardService))]
namespace Forms9Patch.iOS
{
    public class ClipboardService : Forms9Patch.IClipboardService
    {
        const bool TestPre11 = true;

        static internal NSString TypeListUti = new NSString(UTType.CreatePreferredIdentifier(UTType.TagClassMIMEType, "application/f9p-clipboard-typelist", null));

        static ClipboardService()
        {
            UIPasteboard.Notifications.ObserveChanged(OnPasteboardChanged);
            UIPasteboard.Notifications.ObserveRemoved(OnPasteboardChanged);
        }

        static void OnPasteboardChanged(object sender, UIPasteboardChangeEventArgs e)
        {
            Forms9Patch.Clipboard.OnContentChanged(null, EventArgs.Empty);
        }

        #region Entry property
        nint _lastEntryChangeCount = int.MinValue;
        IClipboardEntry _lastEntry;
        public IClipboardEntry Entry
        {
            get
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                if (_lastEntryChangeCount != UIPasteboard.General.ChangeCount)
                {
                    System.Diagnostics.Debug.WriteLine("NOT CACHED: _lastEntryChangeCount=[" + _lastEntryChangeCount + "] ChangeCount=[" + UIPasteboard.General.ChangeCount + "]");
                    _lastEntry = new ClipboardEntry();
                }
                _lastEntryChangeCount = UIPasteboard.General.ChangeCount;
                stopwatch.Stop();
                System.Diagnostics.Debug.WriteLine("\t\t ClipboardService get_Entry elapsed: " + stopwatch.ElapsedMilliseconds);
                return _lastEntry;
            }
            set
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                if (value is Forms9Patch.ClipboardEntry entry)
                {

                    if (!TestPre11 && Forms9Patch.OsInfoService.Version >= new Version(11, 0))
                    {
                        var itemProviders = new List<NSItemProvider>();
                        foreach (var mimeItem in entry.Items)
                        {
                            if (mimeItem.MimeType?.ToNsUti() is NSString nsUti)
                            {
                                NSItemProvider itemProvider = null;
                                if (mimeItem.Value is Uri uri)
                                {
                                    var nsUri = new NSUrl(uri.AbsoluteUri);
                                    itemProvider = new NSItemProvider(nsUri);
                                }
                                else if (mimeItem.Value is FileInfo fileInfo)
                                {
                                    var nsUri = new NSUrl(fileInfo.FullName);
                                    itemProvider = new NSItemProvider(nsUri);
                                }
                                else if (mimeItem.Value.ToNSObject() is NSObject nsObject)
                                    itemProvider = new NSItemProvider(nsObject, nsUti);
                                if (itemProvider != null)
                                    itemProviders.Add(itemProvider);

                            }
                        }
                        if (EntryCaching)
                        {
                            _lastEntry = value;
                            _lastEntryChangeCount = UIPasteboard.General.ChangeCount + 1;
                        }
                        UIPasteboard.General.ItemProviders = itemProviders.ToArray();
                    }
                    else
                    {
                        var items = new List<NSMutableDictionary>();
                        NSMutableDictionary<NSString, NSObject> itemRenditions = null;
                        //NSMutableDictionary itemCSharpTypeMap = null;
                        foreach (var mimeItem in entry.Items)
                        {
                            System.Diagnostics.Debug.WriteLine("\t\t ClipboardService set_Entry 1.0 elapsed: " + stopwatch.ElapsedMilliseconds);
                            //if (mimeItem.MimeType?.ToNsUti() is NSString nsUti && mimeItem.Value.ToNSObject() is NSObject nSObject)
                            if (mimeItem.ToUiPasteboardItem() is KeyValuePair<NSString, NSObject> itemKvp && itemKvp.Key != null)
                            {

                                // if no renditions, add one.
                                // if the current rendition already contains this mimeType, create a new rendition
                                if (itemRenditions == null || itemRenditions.ContainsKey(itemKvp.Key))
                                {
                                    itemRenditions = new NSMutableDictionary<NSString, NSObject>();
                                    items.Add(itemRenditions);
                                }
                                System.Diagnostics.Debug.WriteLine("\t\t ClipboardService set_Entry 1.1 elapsed: " + stopwatch.ElapsedMilliseconds);


                                // some notes here:
                                // when trying to copy a string to iOS Notes app:
                                // - itemRenditions.Add(itemKvp.Key, itemKvp.Value) means the ReturnMimeItem.Value is a byte array BUT it **does** paste correctly into Notes and Mail app
                                // - itemRenditions.Add(itemKvp.Key, plist) pastes the bplist contents into Notes;
                                // - itemRenditions.Add(itemKvp.Key, archiver) pastes the archiver + bplist contents into Notes;

                                /*
                                if (mimeItem.MimeType.StartsWith("image/", StringComparison.InvariantCultureIgnoreCase))
                                    itemRenditions.Add(itemKvp.Key, itemKvp.Value);
                                else
                                {
                                    var archiver = NSKeyedArchiver.ArchivedDataWithRootObject(itemKvp.Value);
                                    itemRenditions.Add(itemKvp.Key, archiver);
                                }
                                */

                                itemRenditions.Add(itemKvp.Key, itemKvp.Value);

                                /*
                                if (mimeItem.MimeType.StartsWith("image/", StringComparison.InvariantCultureIgnoreCase))
                                    itemRenditions.Add(itemKvp.Key, itemKvp.Value);
                                else
                                {
                                    var plist = NSPropertyListSerialization.DataWithPropertyList(itemKvp.Value, NSPropertyListFormat.Binary, NSPropertyListWriteOptions.Immutable, out NSError nSError);
                                    System.Diagnostics.Debug.WriteLine("\t\t ClipboardService set_Entry 1.4 elapsed: " + stopwatch.ElapsedMilliseconds);
                                    itemRenditions.Add(itemKvp.Key, plist);
                                    System.Diagnostics.Debug.WriteLine("\t\t ClipboardService set_Entry 1.5 elapsed: " + stopwatch.ElapsedMilliseconds);
                                }
                                */
                            }
                        }
                        var array = items.ToArray();
                        System.Diagnostics.Debug.WriteLine("\t\t ClipboardService set_Entry 2.1 elapsed: " + stopwatch.ElapsedMilliseconds);
                        if (EntryCaching)
                        {
                            _lastEntry = value;
                            _lastEntryChangeCount = UIPasteboard.General.ChangeCount + 1;
                        }
                        UIPasteboard.General.Items = array;
                        System.Diagnostics.Debug.WriteLine("\t\t ClipboardService set_Entry 2.2 elapsed: " + stopwatch.ElapsedMilliseconds);
                    }

                }

                stopwatch.Stop();
            }
        }

        public bool EntryCaching { get; set; } = false;


        #endregion

    }


    class ItemProvider : NSItemProvider
    {
        Dictionary<string, IMimeItem> _items = new Dictionary<string, IMimeItem>();

        public bool Contains(string mimeType) => _items.ContainsKey(mimeType);

        public void Add(IMimeItem mimeItem) => _items[mimeItem.MimeType] = mimeItem;

        public override bool CanLoadObject(Class aClass)
        {
            return base.CanLoadObject(aClass);
        }

        public override bool HasItemConformingTo(string typeIdentifier)
        {
            //return base.HasItemConformingTo(typeIdentifier);
            foreach (var key in _items.Keys)
                if (key.ToNsUti() == typeIdentifier)
                    return true;
            return false;
        }

        public override Task<NSObject> LoadItemAsync(string typeIdentifier, NSDictionary options)
        {
            return base.LoadItemAsync(typeIdentifier, options);
        }

        public override void LoadItem(string typeIdentifier, NSDictionary options,/* [BlockProxy(typeof(NIDActionArity2V41))] */Action<NSObject, NSError> completionHandler)
        {
            base.LoadItem(typeIdentifier, options, completionHandler);
        }

        public override Task<NSObject> LoadPreviewImageAsync(NSDictionary options)
        {
            return base.LoadPreviewImageAsync(options);
        }

        public override void LoadPreviewImage(NSDictionary options,/* [BlockProxy(typeof(NIDActionArity2V41))]*/ Action<NSObject, NSError> completionHandler)
        {
            base.LoadPreviewImage(options, completionHandler);
        }

        public override void SetPreviewImageHandler(/*[BlockProxy(typeof(NIDNSItemProviderLoadHandler))]*/ NSItemProviderLoadHandler handler)
        {
            base.SetPreviewImageHandler(handler);
        }
    }

}