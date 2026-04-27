namespace MyBlog.API.Services;

public record TagDto(int Id, string Name, string Slug, int PostCount);

public interface ITagService
{
    Task<List<TagDto>> GetAllAsync();
    Task<bool> DeleteAsync(int id);
}
