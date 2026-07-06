-- Migración de esquema introducida por el lado .NET (fix convivencia con el escritorio).
-- Problema: el re-hash gradual del login web (hito 1) pisaba USUARIOS.PASSWD con el hash
-- PBKDF2 "$E1$...", y el Delphi de escritorio (sesion.pas, que descifra PASSWD con
-- EncriptoCadena2) dejaba de reconocer la contraseña.
-- Solución (decisión 2026-07-06): el hash PBKDF2 vive en la columna nueva NPASSWD y
-- PASSWD conserva SIEMPRE el cifrado reversible legacy para el escritorio.
--   * NPASSWD NULL  => usuario que nunca entró por la web: el login valida contra PASSWD
--     (cifrado legacy, o "$E1$" si ya se lo pisó la versión anterior), puebla NPASSWD y
--     repara PASSWD con el cifrado legacy de la contraseña tipeada.
--   * NPASSWD no NULL => el login web valida solo contra NPASSWD.
-- ⚠️ Mientras convivan ambos sistemas PASSWD sigue siendo reversible (excepción
-- consciente a migration_improvements.md §2.7 🔴); al retirar el Delphi se dropea PASSWD.
--
-- Idempotente: solo agrega la columna si aún no existe.
-- Aplicar con: isql -u SYSDBA -p <pass> <ruta esba.gdb> -i este_archivo.sql

SET TERM ^ ;
EXECUTE BLOCK AS
BEGIN
  IF (NOT EXISTS(
        SELECT 1 FROM RDB$RELATION_FIELDS
        WHERE RDB$RELATION_NAME = 'USUARIOS'
          AND RDB$FIELD_NAME = 'NPASSWD')) THEN
    EXECUTE STATEMENT 'ALTER TABLE USUARIOS ADD NPASSWD VARCHAR(60)';
END^
SET TERM ; ^
COMMIT;
