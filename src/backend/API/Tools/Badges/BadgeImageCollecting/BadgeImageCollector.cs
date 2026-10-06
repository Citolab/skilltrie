/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Reflection;
using Models;

namespace API.Tools.Badges.BadgeImageCollecting;

public interface IBadgeImageCollector
{
    public Dictionary<string, BadgeImage> GetBadgeImageCollection();
    public Task CollectAllBadgeImages();
    public Task ParameterizedEntryAdded(ParameterizedBadgeEntry entry);
    IEnumerable<Type> GetAllBadgeImageTypes();
    Task RemoveBadge(string identifier);
}

/// <summary>
/// An interface for classes which listen to the <see cref="BadgeImageCollector"/>
/// </summary>
public interface IBadgeImageCollectorListener
{
    public Task AllBadgeImagesCollected(Dictionary<string, BadgeImage> collectedBadgeImages);
    public Task BadgeImageAdded(BadgeImage badgeImage);
    public Task BadgeImageRemoved(BadgeImage badgeImage);
}

/// <summary>
/// A class which collects all the badge images and creates instances of them
/// using <see cref="ParameterizedBadgeEntry">parameterized badge entries</see>
/// in order to create multiple instances of the same <see cref="BadgeImage"/> with different parameters.<br/>
/// These are then saved and available for other classes using this class. 
/// </summary>
/// <author>Armand Ayar</author>
public class BadgeImageCollector : IBadgeImageCollector
{
    /// <summary>
    /// The collection of badge images. These are saved inside the class.
    /// Therefore, this class should be made a singleton.
    /// </summary>
    private readonly Dictionary<string, BadgeImage> _badgeImageCollection = new ();
    
    /// <summary>
    /// The assembly to search through for all the <see cref="BadgeImage"/> types.
    /// Is settable and public for testing purposes.
    /// </summary>
    public Assembly Assembly = typeof(BadgeImageCollector).Assembly;
    
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BadgeImageCollector> _logger;

    public BadgeImageCollector(IServiceProvider serviceProvider, ILogger<BadgeImageCollector> logger)
    {
        this._logger = logger;
        this._serviceProvider = serviceProvider;
        _ = CollectAllBadgeImages();
    }
    
    /// <summary>
    /// Returns a snapshot of the <see cref="_badgeImageCollection"/>, not the actual collection.
    /// This is to ensure other classes are not accessing and or/modifying the collection instance
    /// of this class which can cause harm.
    /// </summary>
    public Dictionary<string, BadgeImage> GetBadgeImageCollection()
    {
        lock (_badgeImageCollection)
            return new Dictionary<string, BadgeImage>(_badgeImageCollection);
    }
    
    /// <summary>
    /// The main public method of this class. Collects all the badge images as described before and
    /// saves them in the <see cref="_badgeImageCollection"/>. Also calls an event to all the listeners
    /// providing them the snapshot of the filled in collection.
    /// </summary>
    /// <remarks>Clears the current collection</remarks>
    public async Task CollectAllBadgeImages()
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        lock (_badgeImageCollection)
        {
            _badgeImageCollection.Clear();

            IEnumerable<Type> badgeImageTypes = GetAllBadgeImageTypes();

            foreach (Type badgeImageType in badgeImageTypes)
            {
                if (!badgeImageType.IsParameterized())
                    CollectSingleBadgeImage(badgeImageType);
                else
                    CollectParameterizedBadgeImage(badgeImageType, scope);
            }
        }
        
        await CallEvent(
            async listener => await listener.AllBadgeImagesCollected(GetBadgeImageCollection()),
            scope
        );
    }
    
    /// <summary>
    /// Method to notify the image collector that a parameterized entry has been added,
    /// so that it can take action for the collection.
    /// </summary>
    /// <remarks>Assumes entry has all its foreign key objects filled like Topic</remarks>
    public async Task ParameterizedEntryAdded(ParameterizedBadgeEntry entry)
    {
        Type? badgeImageType = GetAllBadgeImageTypes().FirstOrDefault(t => t.Name == entry.BadgeImage);
        if (badgeImageType == null) throw new Exception("badge image type does not exist");
        BadgeImage badgeImage = RegisterParameterizedEntry(badgeImageType, entry);
        
        await CallEvent(
            async listener => await listener.BadgeImageAdded(badgeImage)
        );
    }

    /// <summary>
    /// Removes a badge image from the collection if it exists and notifies listeners if succeeded.
    /// </summary>
    /// <param name="identifier">The identifier of the badge image to remove</param>
    public async Task RemoveBadge(string identifier)
    {
        BadgeImage? badgeImage;
        lock (_badgeImageCollection)
        {
            badgeImage = _badgeImageCollection.GetValueOrDefault(identifier);
            if (badgeImage == null) return;
            RemoveBadgeImage(badgeImage);
        }
        await CallEvent(
            async listener => await listener.BadgeImageRemoved(badgeImage)
        );
    }
    
    /// <summary>
    /// See <see cref="BadgeImageTypeHelper.GetAllBadgeImageTypes"/>
    /// </summary>
    public IEnumerable<Type> GetAllBadgeImageTypes()
    {
        return this.Assembly.GetAllBadgeImageTypes();
    }

    /// <summary>
    /// Utility function to create a scope of all the services registered as listeners and call them.
    /// </summary>
    /// <param name="callListener">An async method to perform an action on the listeners,
    /// this makes it flexible which event to trigger on the listeners</param>
    /// <param name="scope">The current scope which is used. If not provided, creates a new scope</param>
    private async Task CallEvent(Func<IBadgeImageCollectorListener, Task> callListener, IServiceScope? scope = null)
    {
        IServiceScope usingScope = scope ?? _serviceProvider.CreateScope();
        try
        {
            IEnumerable<IBadgeImageCollectorListener> listeners = 
                usingScope.ServiceProvider.GetServices<IBadgeImageCollectorListener>();
            foreach (IBadgeImageCollectorListener listener in listeners)
                await callListener(listener);
        }
        catch (InvalidOperationException)
        {
            return;
        } // no listeners
        finally
        {
            if (scope == null) usingScope.Dispose(); // do not dispose when scope comes from caller method.
        }
    }

    /// <summary>
    /// Creates an instance of a badge image with no parameters and adds it to the collection.
    /// </summary>
    /// <param name="badgeImageType">The type of the badge image to collect</param>
    private void CollectSingleBadgeImage(Type badgeImageType)
    {
        BadgeImage badgeImage = InstantiateBadgeImage(badgeImageType);
        RegisterBadgeImage(badgeImage);
    }

    /// <summary>
    /// Creates zero or more instances of a badge image with one or more parameters and adds it to the collection.
    /// The amount of instances is equal to the amount of parameterized entries found in the database.
    /// </summary>
    /// <param name="badgeImageType">The type of the badge image to collect</param>
    /// <param name="scope">The current scope to use in order to produce the database context.
    /// This context is needed in order to fetch the information of the parameterized entries</param>
    private void CollectParameterizedBadgeImage(Type badgeImageType, IServiceScope scope)
    {
        AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        IEnumerable<ParameterizedBadgeEntry> badgeEntries = badgeImageType.LoadParameterizedEntriesFrom(context);
        foreach (ParameterizedBadgeEntry badgeEntry in badgeEntries)
        {
            try
            {
                RegisterParameterizedEntry(badgeImageType, badgeEntry);
            }
            catch (Exception e)
            {
                _logger.LogError($"Skipping parameterized badge entry (id: ${badgeEntry.Id})" +
                                  $"error: \n" +
                                  $"{e}");
            }
        }
    }

    /// <summary>
    /// Create one parameterized entry of a parameterized badge image and add it to the collection.
    /// </summary>
    private BadgeImage RegisterParameterizedEntry(Type badgeImageType, ParameterizedBadgeEntry badgeEntry)
    {
        BadgeImage badgeImage = CreateFromParameterizedEntry(badgeImageType, badgeEntry);
        RegisterBadgeImage(badgeImage);
        return badgeImage;
    }

    /// <summary>
    /// Takes a badge image type, creates an instance of it and fills the required
    /// parameters based on the given badge entry. Also passes the metadata to the badge image.
    /// </summary>
    /// <returns> The created badge image</returns>
    /// <exception cref="Exception">Is thrown when one required parameter could not be assigned
    /// to the badge image instance.</exception>
    private BadgeImage CreateFromParameterizedEntry(Type badgeImageType, ParameterizedBadgeEntry badgeEntry)
    {
        List<BadgeParameterInfo> badgeParameters = badgeImageType.GetParameters();
        BadgeImage badgeImage = InstantiateBadgeImage(badgeImageType);

        foreach (BadgeParameterInfo badgeParameter in badgeParameters)
        {
            try
            {
                AssignParameter(badgeImage, badgeParameter, badgeEntry);
            }
            catch (Exception e)
            {
                throw new Exception($"Could not create BadgeImage from badgeEntry (id: ${badgeEntry.Id}), " +
                                    $"failed to assign parameter ${badgeParameter.attribute.EntryColumn}\n" +
                                    $"The following error was thrown \n" +
                                    $"{e}");
            }
        }
        
        badgeImage.CreatedFrom = badgeEntry;
        return badgeImage;
    }

    /// <summary>
    /// Assigns one of the parameters of a parameterized badge image.
    /// Uses the <see cref="parameter"/> info to obtain the right value from the badge entry.
    /// </summary>
    /// <param name="badgeImage">The badge image to assign the parameter to</param>
    /// <param name="parameter">The parameter info. See <see cref="BadgeParameterAttribute"/></param>
    /// <param name="badgeEntry">The entry to obtain the information from</param>
    /// <exception cref="ArgumentException">If the required parameter was null in the badgeEntry</exception>
    private void AssignParameter(BadgeImage badgeImage, BadgeParameterInfo parameter, ParameterizedBadgeEntry badgeEntry)
    {
        BadgeParameterAttribute attribute = parameter.attribute;
        PropertyInfo requestedBadgeEntryField = typeof(ParameterizedBadgeEntry)
            .GetProperties()
            .First(p => p.Name == attribute.EntryColumn);

        object? requestedValue = requestedBadgeEntryField.GetValue(badgeEntry);
        if (requestedValue == null) 
            throw new ArgumentException($"Badge parameter {attribute.EntryColumn} was null in badge entry (id: {badgeEntry.Id})");
        parameter.field.SetValue(badgeImage, requestedValue);
    }

    /// <summary>
    /// Creates an instance of a badge image based on its type. No parameters are filled yet.
    /// </summary>
    /// <exception cref="Exception">Is thrown if for some reason the instantiation of the badge image failed</exception>
    /// <remarks>Uses very simple reflection. Basically acts as a wrapper for a try-catch</remarks>
    private BadgeImage InstantiateBadgeImage(Type badgeImageType)
    {
        try
        {
            return (BadgeImage)Activator.CreateInstance(badgeImageType)!;
        }
        catch (Exception e)
        {
            throw new Exception($"Could not construct badge image {badgeImageType.Name}, " +
                                $"make sure there is a empty-arg constructor in the badge image," +
                                $"The following error was thrown \n {e.Message}");
        }
    }

    /// <summary>
    /// Adds a badge image to the collection. The key is the identifier of the badge
    /// and the value is the actual instance of the badge image.
    /// </summary>
    /// <remarks>Dictionary in C# fails automatically when trying to add a key which already exists
    /// Therefore, you cannot add a badge image which already exists</remarks>
    private void RegisterBadgeImage(BadgeImage badgeImage)
    {
        string identifier = badgeImage.BadgeIdentifier();
        lock (_badgeImageCollection)
        {
            if (_badgeImageCollection.ContainsKey(identifier))
                throw new Exception($"Failed to add badge image to collection, " +
                                    $"Another badge with identifier {identifier} already exists");
            _badgeImageCollection.Add(badgeImage.BadgeIdentifier(), badgeImage);
        }
    }

    /// <summary>
    /// Removes a badge image from the collection. Uses the badge identifier for it.
    /// </summary>
    private void RemoveBadgeImage(BadgeImage badgeImage)
    {
        lock (_badgeImageCollection)
        {
            string identifier = badgeImage.BadgeIdentifier();
            _badgeImageCollection.Remove(identifier);
        }
    }
}