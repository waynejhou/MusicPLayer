using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPLayerV2.ViewModels
{
    public class LibraryViewModel : ViewModelBase
    {
        public LibraryViewModel()
        {
            App.Library = this;
        }

        public IEnumerable<LibraryStyle> StyleList => Enum.GetValues(typeof(LibraryStyle)).Cast<LibraryStyle>();
        public LibraryStyle Style { get; set; } = LibraryStyle.Grid;
    }
    public enum LibraryStyle { Grid, CoverFlow }
}
