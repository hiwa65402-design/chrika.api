// Models/Enums.cs
namespace Chrika.Api.Models
{
    public enum GroupType { Public, Private, Temporary }
    public enum GroupRole { Owner, Admin, Member }

    public enum FileType
    {
        ProfilePicture,
        PostImage,
        PostVideo,
        ChatImage,
        ChatVideo,
        ChatAudio
    }
}
