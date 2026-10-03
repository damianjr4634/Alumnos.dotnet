-- Migración de esquema introducida por el lado .NET (área docente, hito 19).
-- Precarga ("borrador") de notas por parte del docente, que secretaría revisa y
-- efectiviza sobre las tablas reales (CURSADA vía regularización; finales vía carga de
-- notas de mesa). Decisiones 2026-10-03:
--   * Juego de tablas NUEVO (no columnas auxiliares en CURSADA/PERMEXA ni JSON): el
--     borrador nunca se mezcla con la verdad, el Delphi no las conoce ni las pisa, y
--     las columnas tipadas permiten listar pendientes, validar y exportar.
--   * El ESTADO vive en la CARGA (una por comisión / una por mesa), no por alumno:
--       BOR = borrador, el docente edita.
--       FIN = finalizado por el docente: sigue viendo, no puede tocar nada.
--       EFE = efectivizado por secretaría sobre las tablas reales.
--     Secretaría puede siempre: modificar los valores cargados, reabrir (FIN -> BOR) o
--     efectivizar. El docente solo edita en BOR y solo si es el TITULAR (COMARM.CODPROFES /
--     MESAS.TITULAR).
--   * Inasistencias como TOTALES del cuatrimestre (TOT_HORAS/INASIST/JUSTIF), igual que la
--     pantalla de regularización. Mismos campos de notas que CURSADA para que efectivizar
--     sea copiar 1:1 y correr los handlers de condición ya existentes (hitos 14 y 15).
--   * Sin FK físicas hacia las tablas legacy (COMARM, CURSADA, MESAS, PERMEXA): el
--     escritorio borra/reescribe esas filas y rompería la integridad. Se guardan los
--     códigos (y CURSADA.INDICE / PERMEXA.INDICE como ayuda) y la app valida.
--     Sí hay FK cabecera -> detalle (ambas nuevas) con borrado en cascada.
--   * Auditoría en columnas: quién/cuándo creó, modificó por última vez, finalizó,
--     reabrió y efectivizó (CODUSU de USUARIOS). La modificación fila a fila del detalle
--     distingue si la hizo el docente o secretaría por el CODUSU.
--   * IDs por generador + trigger BEFORE INSERT, como el resto del esquema.
--
-- Idempotente: cada objeto se crea solo si no existe; se puede re-ejecutar.
-- Aplicar con: isql -u SYSDBA -p <pass> <ruta esba.gdb> -i este_archivo.sql

SET TERM ^ ;

/* ---------- Generadores ---------- */
EXECUTE BLOCK AS
BEGIN
  IF (NOT EXISTS(SELECT 1 FROM RDB$GENERATORS WHERE RDB$GENERATOR_NAME = 'G_DOC_CARGA_COMISION')) THEN
    EXECUTE STATEMENT 'CREATE GENERATOR G_DOC_CARGA_COMISION';
  IF (NOT EXISTS(SELECT 1 FROM RDB$GENERATORS WHERE RDB$GENERATOR_NAME = 'G_DOC_CARGA_COMISION_DET')) THEN
    EXECUTE STATEMENT 'CREATE GENERATOR G_DOC_CARGA_COMISION_DET';
  IF (NOT EXISTS(SELECT 1 FROM RDB$GENERATORS WHERE RDB$GENERATOR_NAME = 'G_DOC_CARGA_MESA')) THEN
    EXECUTE STATEMENT 'CREATE GENERATOR G_DOC_CARGA_MESA';
  IF (NOT EXISTS(SELECT 1 FROM RDB$GENERATORS WHERE RDB$GENERATOR_NAME = 'G_DOC_CARGA_MESA_DET')) THEN
    EXECUTE STATEMENT 'CREATE GENERATOR G_DOC_CARGA_MESA_DET';
END^
COMMIT^

/* ---------- Comisiones: cabecera ---------- */
EXECUTE BLOCK AS
BEGIN
  IF (NOT EXISTS(SELECT 1 FROM RDB$RELATIONS WHERE RDB$RELATION_NAME = 'DOC_CARGA_COMISION')) THEN
    EXECUTE STATEMENT '
      CREATE TABLE DOC_CARGA_COMISION (
        ID                INTEGER      NOT NULL,
        CARRE             VARCHAR(6)   NOT NULL,
        CUTUCO            SMALLINT     NOT NULL,
        COD_MAT           CHAR(2)      NOT NULL,
        CUA_ANIO          CHAR(3)      NOT NULL,
        CODPROFES         CHAR(3)      NOT NULL,
        ESTADO            CHAR(3)      DEFAULT ''BOR'' NOT NULL,
        OBSERV            VARCHAR(500),
        FEC_ALTA          TIMESTAMP    NOT NULL,
        CODUSU_ALTA       INTEGER      NOT NULL,
        FEC_MODIF         TIMESTAMP,
        CODUSU_MODIF      INTEGER,
        FEC_FINAL         TIMESTAMP,
        CODUSU_FINAL      INTEGER,
        FEC_REAPERTURA    TIMESTAMP,
        CODUSU_REAPERTURA INTEGER,
        FEC_EFECTIVO      TIMESTAMP,
        CODUSU_EFECTIVO   INTEGER,
        CONSTRAINT PK_DOC_CARGA_COMISION PRIMARY KEY (ID),
        CONSTRAINT UNQ_DOC_CARGA_COMISION UNIQUE (CARRE, CUTUCO, COD_MAT, CUA_ANIO),
        CONSTRAINT CHK_DOC_CARGA_COMISION_ESTADO CHECK (ESTADO IN (''BOR'', ''FIN'', ''EFE'')))';
END^
COMMIT^

/* ---------- Comisiones: detalle (un alumno de la comisión) ---------- */
EXECUTE BLOCK AS
BEGIN
  IF (NOT EXISTS(SELECT 1 FROM RDB$RELATIONS WHERE RDB$RELATION_NAME = 'DOC_CARGA_COMISION_DET')) THEN
    EXECUTE STATEMENT '
      CREATE TABLE DOC_CARGA_COMISION_DET (
        ID             INTEGER       NOT NULL,
        CARGA_ID       INTEGER       NOT NULL,
        COD_ALU        CHAR(11)      NOT NULL,
        CURSADA_INDICE INTEGER,
        TP_EVA         NUMERIC(5, 2),
        RECUP          NUMERIC(5, 2),
        TP_EVA2        NUMERIC(5, 2),
        RECUP2         NUMERIC(5, 2),
        TP_EVA3        NUMERIC(5, 2),
        TOT_HORAS      NUMERIC(3, 0),
        INASIST        NUMERIC(3, 0),
        JUSTIF         NUMERIC(3, 0),
        OBSERV         VARCHAR(500),
        FEC_MODIF      TIMESTAMP     NOT NULL,
        CODUSU_MODIF   INTEGER       NOT NULL,
        CONSTRAINT PK_DOC_CARGA_COMISION_DET PRIMARY KEY (ID),
        CONSTRAINT UNQ_DOC_CARGA_COMISION_DET UNIQUE (CARGA_ID, COD_ALU),
        CONSTRAINT FK_DOC_CARGA_COMISION_DET FOREIGN KEY (CARGA_ID)
          REFERENCES DOC_CARGA_COMISION (ID) ON DELETE CASCADE)';
END^
COMMIT^

/* ---------- Mesas: cabecera ---------- */
EXECUTE BLOCK AS
BEGIN
  IF (NOT EXISTS(SELECT 1 FROM RDB$RELATIONS WHERE RDB$RELATION_NAME = 'DOC_CARGA_MESA')) THEN
    EXECUTE STATEMENT '
      CREATE TABLE DOC_CARGA_MESA (
        ID                INTEGER      NOT NULL,
        CARRE             VARCHAR(6)   NOT NULL,
        MESA              INTEGER      NOT NULL,
        CODPROFES         CHAR(3)      NOT NULL,
        ESTADO            CHAR(3)      DEFAULT ''BOR'' NOT NULL,
        OBSERV            VARCHAR(500),
        FEC_ALTA          TIMESTAMP    NOT NULL,
        CODUSU_ALTA       INTEGER      NOT NULL,
        FEC_MODIF         TIMESTAMP,
        CODUSU_MODIF      INTEGER,
        FEC_FINAL         TIMESTAMP,
        CODUSU_FINAL      INTEGER,
        FEC_REAPERTURA    TIMESTAMP,
        CODUSU_REAPERTURA INTEGER,
        FEC_EFECTIVO      TIMESTAMP,
        CODUSU_EFECTIVO   INTEGER,
        CONSTRAINT PK_DOC_CARGA_MESA PRIMARY KEY (ID),
        CONSTRAINT UNQ_DOC_CARGA_MESA UNIQUE (CARRE, MESA),
        CONSTRAINT CHK_DOC_CARGA_MESA_ESTADO CHECK (ESTADO IN (''BOR'', ''FIN'', ''EFE'')))';
END^
COMMIT^

/* ---------- Mesas: detalle (un alumno con permiso en la mesa) ---------- */
EXECUTE BLOCK AS
BEGIN
  IF (NOT EXISTS(SELECT 1 FROM RDB$RELATIONS WHERE RDB$RELATION_NAME = 'DOC_CARGA_MESA_DET')) THEN
    EXECUTE STATEMENT '
      CREATE TABLE DOC_CARGA_MESA_DET (
        ID             INTEGER       NOT NULL,
        CARGA_ID       INTEGER       NOT NULL,
        COD_ALU        CHAR(11)      NOT NULL,
        COD_MAT        CHAR(2)       NOT NULL,
        PERMEXA_INDICE INTEGER,
        NOTA           NUMERIC(5, 2),
        AUSENTE        CHAR(1)       DEFAULT ''N'' NOT NULL,
        OBSERV         VARCHAR(500),
        FEC_MODIF      TIMESTAMP     NOT NULL,
        CODUSU_MODIF   INTEGER       NOT NULL,
        CONSTRAINT PK_DOC_CARGA_MESA_DET PRIMARY KEY (ID),
        CONSTRAINT UNQ_DOC_CARGA_MESA_DET UNIQUE (CARGA_ID, COD_ALU, COD_MAT),
        CONSTRAINT CHK_DOC_CARGA_MESA_DET_AUSENTE CHECK (AUSENTE IN (''S'', ''N'')),
        CONSTRAINT FK_DOC_CARGA_MESA_DET FOREIGN KEY (CARGA_ID)
          REFERENCES DOC_CARGA_MESA (ID) ON DELETE CASCADE)';
END^
COMMIT^

/* ---------- Índices de búsqueda (cargas de un docente / pendientes de secretaría) ---------- */
EXECUTE BLOCK AS
BEGIN
  IF (NOT EXISTS(SELECT 1 FROM RDB$INDICES WHERE RDB$INDEX_NAME = 'IDX_DOC_CARGA_COMISION_PROF')) THEN
    EXECUTE STATEMENT 'CREATE INDEX IDX_DOC_CARGA_COMISION_PROF ON DOC_CARGA_COMISION (CODPROFES, ESTADO)';
  IF (NOT EXISTS(SELECT 1 FROM RDB$INDICES WHERE RDB$INDEX_NAME = 'IDX_DOC_CARGA_MESA_PROF')) THEN
    EXECUTE STATEMENT 'CREATE INDEX IDX_DOC_CARGA_MESA_PROF ON DOC_CARGA_MESA (CODPROFES, ESTADO)';
END^
COMMIT^

/* ---------- Triggers de identidad (ID por generador si viene nulo) ---------- */
EXECUTE BLOCK AS
BEGIN
  IF (NOT EXISTS(SELECT 1 FROM RDB$TRIGGERS WHERE RDB$TRIGGER_NAME = 'DOC_CARGA_COMISION_BI0')) THEN
    EXECUTE STATEMENT '
      CREATE TRIGGER DOC_CARGA_COMISION_BI0 FOR DOC_CARGA_COMISION
      ACTIVE BEFORE INSERT POSITION 0
      AS
      BEGIN
        IF (NEW.ID IS NULL) THEN NEW.ID = GEN_ID(G_DOC_CARGA_COMISION, 1);
      END';
  IF (NOT EXISTS(SELECT 1 FROM RDB$TRIGGERS WHERE RDB$TRIGGER_NAME = 'DOC_CARGA_COMISION_DET_BI0')) THEN
    EXECUTE STATEMENT '
      CREATE TRIGGER DOC_CARGA_COMISION_DET_BI0 FOR DOC_CARGA_COMISION_DET
      ACTIVE BEFORE INSERT POSITION 0
      AS
      BEGIN
        IF (NEW.ID IS NULL) THEN NEW.ID = GEN_ID(G_DOC_CARGA_COMISION_DET, 1);
      END';
  IF (NOT EXISTS(SELECT 1 FROM RDB$TRIGGERS WHERE RDB$TRIGGER_NAME = 'DOC_CARGA_MESA_BI0')) THEN
    EXECUTE STATEMENT '
      CREATE TRIGGER DOC_CARGA_MESA_BI0 FOR DOC_CARGA_MESA
      ACTIVE BEFORE INSERT POSITION 0
      AS
      BEGIN
        IF (NEW.ID IS NULL) THEN NEW.ID = GEN_ID(G_DOC_CARGA_MESA, 1);
      END';
  IF (NOT EXISTS(SELECT 1 FROM RDB$TRIGGERS WHERE RDB$TRIGGER_NAME = 'DOC_CARGA_MESA_DET_BI0')) THEN
    EXECUTE STATEMENT '
      CREATE TRIGGER DOC_CARGA_MESA_DET_BI0 FOR DOC_CARGA_MESA_DET
      ACTIVE BEFORE INSERT POSITION 0
      AS
      BEGIN
        IF (NEW.ID IS NULL) THEN NEW.ID = GEN_ID(G_DOC_CARGA_MESA_DET, 1);
      END';
END^
COMMIT^

SET TERM ; ^

/* ---------- Comentarios (sin acentos: el charset de isql puede no coincidir con el de la base) ---------- */
COMMENT ON TABLE DOC_CARGA_COMISION IS 'Precarga del docente titular sobre una comision (notas de cursado + totales de horas/inasistencias). Estado BOR/FIN/EFE. Secretaria efectiviza sobre CURSADA via regularizacion';
COMMENT ON COLUMN DOC_CARGA_COMISION.ESTADO IS 'BOR borrador (docente edita) / FIN finalizado por el docente (solo lectura) / EFE efectivizado por secretaria';
COMMENT ON COLUMN DOC_CARGA_COMISION.CODPROFES IS 'Docente titular de la comision al crear la carga (COMARM.CODPROFES)';
COMMENT ON TABLE DOC_CARGA_COMISION_DET IS 'Un alumno de la comision: mismos campos de notas que CURSADA para efectivizar 1:1. CURSADA_INDICE es ayuda, no FK';
COMMENT ON TABLE DOC_CARGA_MESA IS 'Precarga del docente titular de una mesa de examen (notas de final). Estado BOR/FIN/EFE. Secretaria efectiviza via carga de notas de finales';
COMMENT ON COLUMN DOC_CARGA_MESA.CODPROFES IS 'Docente titular de la mesa al crear la carga (MESAS.TITULAR)';
COMMENT ON TABLE DOC_CARGA_MESA_DET IS 'Un alumno con permiso en la mesa: nota de final o ausente. PERMEXA_INDICE es ayuda, no FK';
COMMENT ON COLUMN DOC_CARGA_MESA_DET.AUSENTE IS 'S = el alumno no se presento (distinto de nota sin cargar)';
COMMIT;
