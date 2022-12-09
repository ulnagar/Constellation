using Constellation.Core.Models.GroupTutorials;

namespace Constellation.Core.Abstractions;

public interface IGroupTutorialRepository
{
    void Insert(GroupTutorial tutorial);
}
