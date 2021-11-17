using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRtDb.DTO.Languages
{
    public class LanguageResource
    {
        public long Id { get; set; }
        public string Language { get; set; }
        public List<LangRes> Resources { get; set; }
    }
}
