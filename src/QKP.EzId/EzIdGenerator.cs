using System;

namespace QKP.EzId
{
    /// <summary>
    /// Generates identifiers of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The ID type, which must implement IEzIdType{T}.
    /// </typeparam>
    public class EzIdGenerator<T> where T : IEzIdType<T>
    {
        private readonly IdGenerator _generator;

        /// <summary>
        /// Constructs an instance of <see cref="EzIdGenerator{T}"/>.
        /// </summary>
        /// <param name="generatorId">A unique generator identifier that must be unique per concurrent process that can generate Ids.</param>
        public EzIdGenerator(long generatorId)
        {
            _generator = new IdGenerator(generatorId);
        }

        /// <summary>
        /// Gets the next identifier.
        /// </summary>
        /// <returns>An instance of type T.</returns>
        /// <exception cref="InvalidOperationException">Thrown when instance of type T could not be created.</exception>
        public virtual T GetNextId()
        {
            return (T)Activator.CreateInstance(typeof(T), _generator.GetNextId())! ?? throw new InvalidOperationException($"Could not construct type {typeof(T).FullName}.");
        }
    }
}
