-- Migración de esquema introducida por el lado .NET (hito 12.3 ampliado — perfiles de acceso).
-- Problema: USUARIOS solo modela personal de secretaría. La web va a tener además usuarios
-- docentes (área /docente) y, más adelante, alumnos (portal). Decisión 2026-10-02: un solo
-- login y una sola tabla de credenciales; el tipo de usuario y su vínculo viven en USUARIOS.
--
--   * TIPO CHAR(3) NOT NULL DEFAULT 'SEC': 'SEC' = secretaría (comportamiento actual),
--     'DOC' = docente, 'ALU' = alumno. Las filas existentes quedan en 'SEC'. El Delphi de
--     escritorio no conoce la columna: sus altas caen en el DEFAULT.
--   * CODPROFES CHAR(3): obligatorio cuando TIPO='DOC', apunta a DOCENTES.CODPROFES.
--   * ALU_CARRE VARCHAR(6) + ALU_COD_ALU CHAR(11): obligatorios cuando TIPO='ALU',
--     apuntan a la PK compuesta de ALUMNOS (CARRE, COD_ALU).
--
-- Sin FK físicas ni CHECK de consistencia, como el resto del esquema: la regla
-- "DOC exige CODPROFES / ALU exige ALU_CARRE+ALU_COD_ALU" la aplica el validador del
-- ABM de usuarios (.NET). El escritorio sigue escribiendo USUARIOS y un CHECK con
-- columnas que no conoce podría romperle altas.
-- ⚠️ Mientras conviva el escritorio: un usuario DOC/ALU NO debe poder entrar al Delphi,
-- que valida PASSWD sin mirar TIPO. El alta web de esos usuarios deja en PASSWD un
-- sentinela indescifrable (misma técnica que '/' para blanqueo) y solo los SEC
-- sincronizan PASSWD al cambiar la contraseña.
--
-- Idempotente: cada columna se agrega solo si aún no existe; se puede re-ejecutar.
-- Aplicar con: isql -u SYSDBA -p <pass> <ruta esba.gdb> -i este_archivo.sql

SET TERM ^ ;
EXECUTE BLOCK AS
BEGIN
  IF (NOT EXISTS(
        SELECT 1 FROM RDB$RELATION_FIELDS
        WHERE RDB$RELATION_NAME = 'USUARIOS'
          AND RDB$FIELD_NAME = 'TIPO')) THEN
    EXECUTE STATEMENT 'ALTER TABLE USUARIOS ADD TIPO CHAR(3) DEFAULT ''SEC'' NOT NULL';

  IF (NOT EXISTS(
        SELECT 1 FROM RDB$RELATION_FIELDS
        WHERE RDB$RELATION_NAME = 'USUARIOS'
          AND RDB$FIELD_NAME = 'CODPROFES')) THEN
    EXECUTE STATEMENT 'ALTER TABLE USUARIOS ADD CODPROFES CHAR(3)';

  IF (NOT EXISTS(
        SELECT 1 FROM RDB$RELATION_FIELDS
        WHERE RDB$RELATION_NAME = 'USUARIOS'
          AND RDB$FIELD_NAME = 'ALU_CARRE')) THEN
    EXECUTE STATEMENT 'ALTER TABLE USUARIOS ADD ALU_CARRE VARCHAR(6)';

  IF (NOT EXISTS(
        SELECT 1 FROM RDB$RELATION_FIELDS
        WHERE RDB$RELATION_NAME = 'USUARIOS'
          AND RDB$FIELD_NAME = 'ALU_COD_ALU')) THEN
    EXECUTE STATEMENT 'ALTER TABLE USUARIOS ADD ALU_COD_ALU CHAR(11)';
END^
SET TERM ; ^
COMMIT;

-- Red de seguridad: si alguna fila quedó con TIPO nulo (no debería, el DEFAULT cubre a las
-- existentes), se la trata como secretaría. Inocuo al re-ejecutar.
UPDATE USUARIOS SET TIPO = 'SEC' WHERE TIPO IS NULL;
COMMIT;

-- Comentarios de columna (documentación en la propia base, como hace el DDL original).
-- Sin acentos a propósito: el charset de conexión de isql puede no coincidir con el de la base.
COMMENT ON COLUMN USUARIOS.TIPO IS 'Perfil de acceso web: SEC secretaria, DOC docente, ALU alumno';
COMMENT ON COLUMN USUARIOS.CODPROFES IS 'Docente vinculado (DOCENTES.CODPROFES) cuando TIPO=DOC';
COMMENT ON COLUMN USUARIOS.ALU_CARRE IS 'Carrera del alumno vinculado (ALUMNOS.CARRE) cuando TIPO=ALU';
COMMENT ON COLUMN USUARIOS.ALU_COD_ALU IS 'Codigo del alumno vinculado (ALUMNOS.COD_ALU) cuando TIPO=ALU';
COMMIT;
