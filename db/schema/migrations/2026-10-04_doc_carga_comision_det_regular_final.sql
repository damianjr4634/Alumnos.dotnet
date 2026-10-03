-- Migración de esquema introducida por el lado .NET (área docente, hito 19 — corrección).
-- La precarga por comisión debe pedirle al docente exactamente los campos que secretaría
-- edita en la regularización de esa carrera (2026-10-04):
--   * Terciarias (CARRERA.TIPO='TER'): 1° y 2° parcial, recuperatorio, horas, inasistencias,
--     justificadas.
--   * Bachillerato (CARRE='BAC'): 1° y 2° bimestre, recuperatorio, nota "a regularizar"
--     (CURSADA.REGULAR), horas, inasistencias, justificadas.
--   * Secundario (CARRE 333/650): 1°, 2° y 3° trimestre, horas, inasistencias (diciembre y
--     marzo son exámenes de secretaría).
--   * CNA: nota final (CURSADA.FINAL1).
-- La tabla original no tenía columna para la nota "a regularizar" ni para la nota final:
-- se agregan REGULAR y FINAL1 con los mismos tipos que CURSADA. RECUP2 queda (ninguna
-- variante lo usa; no se borra una columna ya aplicada en producción).
--
-- Idempotente: cada columna se agrega solo si aún no existe; se puede re-ejecutar.
-- Aplicar con: isql -u SYSDBA -p <pass> <ruta esba.gdb> -i este_archivo.sql

SET TERM ^ ;
EXECUTE BLOCK AS
BEGIN
  IF (NOT EXISTS(
        SELECT 1 FROM RDB$RELATION_FIELDS
        WHERE RDB$RELATION_NAME = 'DOC_CARGA_COMISION_DET'
          AND RDB$FIELD_NAME = 'REGULAR')) THEN
    EXECUTE STATEMENT 'ALTER TABLE DOC_CARGA_COMISION_DET ADD REGULAR NUMERIC(5, 2)';

  IF (NOT EXISTS(
        SELECT 1 FROM RDB$RELATION_FIELDS
        WHERE RDB$RELATION_NAME = 'DOC_CARGA_COMISION_DET'
          AND RDB$FIELD_NAME = 'FINAL1')) THEN
    EXECUTE STATEMENT 'ALTER TABLE DOC_CARGA_COMISION_DET ADD FINAL1 NUMERIC(5, 2)';
END^
SET TERM ; ^
COMMIT;

COMMENT ON COLUMN DOC_CARGA_COMISION_DET.REGULAR IS 'Nota a regularizar (bachillerato), espejo de CURSADA.REGULAR';
COMMENT ON COLUMN DOC_CARGA_COMISION_DET.FINAL1 IS 'Nota final (CNA), espejo de CURSADA.FINAL1';
COMMIT;
