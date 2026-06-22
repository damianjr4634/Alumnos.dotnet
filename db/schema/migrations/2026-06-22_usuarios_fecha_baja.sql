-- Migración de esquema introducida por el lado .NET (hito 10.1a — ABM de usuarios).
-- Agrega baja lógica a USUARIOS: FECHA_BAJ DATE, NULL = usuario activo.
-- El login .NET filtra los usuarios con FECHA_BAJ no nula (el Delphi legacy
-- no conoce esta columna; un usuario dado de baja seguiría entrando por el
-- sistema viejo mientras corra en paralelo — trade-off aceptado, 2026-06-22).
--
-- Idempotente: solo agrega la columna si aún no existe.
-- Aplicar con: isql -u SYSDBA -p <pass> <ruta esba.gdb> -i este_archivo.sql

SET TERM ^ ;
EXECUTE BLOCK AS
BEGIN
  IF (NOT EXISTS(
        SELECT 1 FROM RDB$RELATION_FIELDS
        WHERE RDB$RELATION_NAME = 'USUARIOS'
          AND RDB$FIELD_NAME = 'FECHA_BAJ')) THEN
    EXECUTE STATEMENT 'ALTER TABLE USUARIOS ADD FECHA_BAJ DATE';
END^
SET TERM ; ^
COMMIT;
