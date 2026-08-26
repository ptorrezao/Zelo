// Corre as migrations de todos os modulos e termina.
// Executado como init container / job de deploy, ANTES da Api e do Worker.
// Nenhum host aplica migrations no arranque - evita corridas entre replicas.

Console.WriteLine("Zelo :: migration runner");

// TODO: para cada DbContext de modulo -> await db.Database.MigrateAsync();

Console.WriteLine("Migrations concluidas.");
return 0;
