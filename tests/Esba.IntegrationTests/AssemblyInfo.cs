using Xunit;

// Los tests de integración comparten la misma base Firebird (escriben, hacen
// rollback y limpian). Correrlos en paralelo provoca interferencia entre
// transacciones (un preview con rollback puede ver lo que otra clase commitea).
// Se serializan a nivel de assembly.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
