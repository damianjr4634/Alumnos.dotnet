/*
 * Tabla ANALITIC — histórico académico / analítico del alumno.
 *
 * Extraído de la base real (isql -x sobre /pool/firebird/esba.gdb) en el hito 9.3a
 * (modelado de ANALITIC, prerrequisito de la migración de equivalencias). Primera
 * tabla versionada del repo: hasta ahora db/schema/ solo versionaba procedures/.
 *
 * Notas para la migración (mapeadas a Analitico.cs / AnaliticoConfiguration.cs):
 *  - PK compuesta (CARRE, COD_ALU, COD_MAT). INDICE es un surrogate asignado por
 *    el trigger ANALITIC_BI0 desde el generador G_ANALITIC (no se setea al insertar).
 *  - ANALITIC_BI0: además valida que la materia NO exista en CURSADA (excepción) y
 *    que COD_MAT no sea vacío; setea ULTMOD.
 *  - ANALITIC_BIU (pos 1): rellena ACTINT/ACTDGE con LPAD(...,15,'0') — el padding
 *    del número de actuación lo hace la base, no el código C#.
 *  - LOG_ANALITIC_BIUD: auditoría a LOG_ANALITIC en cada INSERT/UPDATE/DELETE.
 */

CREATE GENERATOR G_ANALITIC;

CREATE TABLE ANALITIC (
        COD_ALU CHAR(11) NOT NULL,
        APELLIDO CHAR(25),
        COD_MAT CHAR(2) NOT NULL,
        CUA_ANIO CHAR(3),
        NOTA_MAT NUMERIC(5, 2),
        FEC_FINAL DATE,
        MATRIZ CHAR(5),
        CONDICION CHAR(15),
        INSTITUT CHAR(30),
        CARAC CHAR(6),
        ACTINT VARCHAR(15),
        ACTDGE VARCHAR(15),
        ACTSNE VARCHAR(10),
        CARRE VARCHAR(6) NOT NULL,
        COLEGIO CHAR(40),
        "PLAN" CHAR(40),
        A_C CHAR(1),
        NREG NUMERIC(5, 0),
        INDICE INTEGER NOT NULL,
        USUARIO VARCHAR(15),
        FEQDOCE VARCHAR(3),
        FEQMATE VARCHAR(50),
        FEQCARRE VARCHAR(100),
        FEQINST VARCHAR(100),
        ULTMOD TIMESTAMP,
        FACTFIN VARCHAR(10),
        FEXDESCRI VARCHAR(200),
CONSTRAINT PK_ANALITIC PRIMARY KEY (CARRE, COD_ALU, COD_MAT));

CREATE INDEX ANALITIC_ACTINT ON ANALITIC (ACTINT);
CREATE INDEX ANALITIC_CARRE ON ANALITIC (CARRE);
CREATE INDEX ANALITIC_CODALU_CARRE ON ANALITIC (COD_ALU, CARRE);
CREATE INDEX ANALITIC_IDX1 ON ANALITIC (FEQDOCE);
CREATE INDEX ANALITIC_IDX2 ON ANALITIC (INDICE);

SET TERM ^ ;

CREATE TRIGGER ANALITIC_BI0 FOR ANALITIC
ACTIVE BEFORE INSERT POSITION 0
AS
DECLARE VARIABLE MATERIA VARCHAR(40);
begin
  new.INDICE = GEN_ID(G_ANALITIC,1);
  new.ULTMOD=CURRENT_TIMESTAMP;

  IF (TRIM(new.CUA_ANIO)='') THEN
     new.CUA_ANIO=NULL;

  IF (EXISTS(SELECT 1 FROM CURSADA C WHERE C.COD_ALU=new.COD_ALU AND C.CARRE=new.CARRE AND C.COD_MAT=new.COD_MAT)) THEN BEGIN
     SELECT SUBSTRING(M.DESCRIPCI FROM 1 FOR 30)
     FROM MATERIAS M
     WHERE M.CODCARRE=new.CARRE AND M.CODMATERI=new.COD_MAT
     INTO :MATERIA;
     EXCEPTION E_CUSTOM_ERR 'La materia, '||MATERIA||', no puede estar en cursada a la vez borrela primero';
  END

  if (new.COD_MAT is null or trim(new.COD_MAT) = '' ) then
        EXCEPTION E_CUSTOM_ERR 'El codigo de materia no puede estar vacio';
end ^

CREATE TRIGGER ANALITIC_BU0 FOR ANALITIC
ACTIVE BEFORE UPDATE POSITION 0
AS
begin
  new.ULTMOD=CURRENT_TIMESTAMP;
  IF (COALESCE(new.MATRIZ,'')='' AND COALESCE(old.MATRIZ,'')<>'') THEN
    EXCEPTION E_CUSTOM_ERR 'No se puede blanquear un numero de matriz del un alumno';
end ^

CREATE TRIGGER ANALITIC_BIU0 FOR ANALITIC
ACTIVE BEFORE INSERT OR UPDATE POSITION 0
AS
begin
  if (coalesce(new.carre,'')='') then
    exception e_custom_err 'la carrera no puede estar vacia';
end ^

CREATE TRIGGER ANALITIC_BIU FOR ANALITIC
ACTIVE BEFORE INSERT OR UPDATE POSITION 1
AS
begin
  IF (new.ACTINT IS NOT NULL and new.actint<>'') THEN
    new.ACTINT=LPAD(TRIM(new.ACTINT),15,'0');

  IF (new.ACTDGE IS NOT NULL and new.actdge <> '') THEN
    new.ACTDGE =LPAD(TRIM(new.ACTDGE),15,'0');

   new.ULTMOD=CURRENT_TIMESTAMP;

  IF (TRIM(new.CUA_ANIO)='') THEN
     new.CUA_ANIO=NULL;

  if (new.fec_final = '1899-12-30') then
    new.fec_final = null;
end ^

SET TERM ; ^
