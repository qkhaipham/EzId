using System;

namespace QKP.EzId
{
    /// <summary>
    /// Generates identifiers of type <see cref="EzId"/>.
    /// </summary>
    public class EzIdGenerator<T>
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
