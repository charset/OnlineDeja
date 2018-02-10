using System.Linq;
using System.Threading.Tasks;

namespace OnlineDeja.Models {
    public class UserInfo {
        public string OpenId { get; set; }
        public string NickName { get; set; }
        public string Gender { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string Country { get; set; }
        public string AvatarUrl { get; set; }
        public string UnionId { get; set; }
    }
}