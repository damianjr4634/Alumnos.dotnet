SET TERM ^ ;

ALTER PROCEDURE XXX_REGULARIZACION_MAT_333 (CARRE VARCHAR(6) CHARACTER SET NONE,
CODALU VARCHAR(11) CHARACTER SET NONE,
CODMAT VARCHAR(2) CHARACTER SET NONE,
CONDIORG VARCHAR(20) CHARACTER SET NONE,
USUARIO INTEGER)
RETURNS (FERRCOD INTEGER,
FERRMSG VARCHAR(3000) CHARACTER SET NONE)
AS 
declare variable CUTUCO smallint;
declare variable CUA_ANIO char(3);
declare variable CONDICION char(15);
declare variable TP_EVA numeric(5,2);
declare variable RECUP numeric(5,2);
declare variable TP_EVA2 numeric(5,2);
declare variable RECUP2 numeric(5,2);
declare variable REGULAR numeric(5,2);
declare variable TOT_HORAS smallint;
declare variable INASIST smallint;
declare variable JUSTIF smallint;
declare variable FINAL1 numeric(5,2);
declare variable FECHA1 date;
declare variable FINAL2 numeric(5,2);
declare variable FECHA2 date;
declare variable APELLIDO char(25);
declare variable MATRIZ char(5);
declare variable NOM_APE varchar(40);
declare variable TP_EVA3 numeric(5,2);
declare variable PROM numeric(5,2);
declare variable FEC_EVA1 date;
declare variable FEC_EVA2 date;
declare variable FEC_EVA3 date;
declare variable FAL_EVA1 numeric(5,2);
declare variable FAL_EVA2 numeric(5,2);
declare variable FAL_EVA3 numeric(5,2);
declare variable NOTADIC numeric(5,2);
declare variable NOTAMAR numeric(5,2);
declare variable FECHDIC date;
declare variable FECHMAR date;
declare variable NOTAFIN numeric(5,2);
declare variable NOTAFIN_FECHA date;
begin
SELECT CUTUCO, CUA_ANIO, CONDICION, TP_EVA, RECUP, TP_EVA2,
       RECUP2, REGULAR, TOT_HORAS, INASIST, JUSTIF, FINAL1, FECHA1, FINAL2, FECHA2,
       APELLIDO, MATRIZ, NOM_APE, TP_EVA3, PROM, FEC_EVA1, FEC_EVA2, FEC_EVA3,
       FAL_EVA1, FAL_EVA2, FAL_EVA3, NOTADIC, NOTAMAR, FECHDIC, FECHMAR
FROM "$$$CURSADA" X
WHERE COD_ALU=:CODALU AND X.COD_MAT=:CODMAT AND X.USUARIO=:USUARIO
INTO :CUTUCO, :CUA_ANIO, :CONDICION, :TP_EVA, :RECUP, :TP_EVA2,
     :RECUP2, :REGULAR, :TOT_HORAS, :INASIST, :JUSTIF, :FINAL1, :FECHA1, :FINAL2, :FECHA2,
     :APELLIDO, :MATRIZ, :NOM_APE, :TP_EVA3, :PROM, :FEC_EVA1, :FEC_EVA2, :FEC_EVA3,
     :FAL_EVA1, :FAL_EVA2, :FAL_EVA3, :NOTADIC, :NOTAMAR, :FECHDIC, :FECHMAR;

NOTAFIN=0;
/*  calculo promedio
   TP_EVA  1er trim
   TP_EVA2 2do trim
   TP_EVA3 3er trim
   NOTAMAR
   NOTADIC
*/
/* ****************************/
if (TP_EVA2 >= 6 AND TP_EVA2 <> 99) then BEGIN
    CONDICION='REGULAR';
    NOTAFIN = TP_EVA2;
    NOTAFIN_FECHA = FEC_EVA2;
END
else IF ((TP_EVA=99 AND TP_EVA2=99) or (TP_EVA2 > 0 AND TP_EVA2 < 6)) then BEGIN
    CONDICION='ENPROCESO';

    if (CONDICION='ENPROCESO') then BEGIN
        IF (NOTADIC >= 6) THEN BEGIN
            CONDICION='REGULAR';
            NOTAFIN = NOTADIC;
            NOTAFIN_FECHA = fechdic;
            IF (FECHDIC IS NULL) THEN BEGIN
                 FERRCOD=2;
                 FERRMSG='Si carga nota de diciembre cargue la fecha';
                 SUSPEND;
                 EXIT;
            END
        END
        ELSE IF (NOTAMAR >= 6) THEN BEGIN
            CONDICION='REGULAR';
            NOTAFIN = NOTAMAR;
            NOTAFIN_FECHA = FECHMAR;
            IF (FECHMAR IS NULL) THEN BEGIN
               FERRCOD=2;
               FERRMSG='Si carga nota de marzo cargue la fecha';
               SUSPEND;
               EXIT;
            END
        END
        ELSE IF ((NOTADIC > 0 AND NOTADIC < 6) or (NOTAMAR > 0 AND NOTAMAR < 6) or NOTADIC = 99 or NOTAMAR = 99) THEN
            CONDICION='PREVIA';
    END
END
ELSE
    CONDICION=CONDIORG;


/*IF (PROM = 0) THEN BEGIN
       IF (NOTAMAR <= 0) THEN
          CONDICION = 'MARZO';
       ELSE BEGIN
            IF ((NOTAMAR = 99) OR (NOTAMAR < 6)) THEN
              CONDICION = 'PREVIA';
            ELSE BEGIN
              CONDICION = 'REGULAR';
              NOTAFIN=NOTAMAR;
              NOTAFIN_FECHA = FECHMAR;
            END
       END
END
ELSE IF (PROM > 0) THEN BEGIN
    IF (PROM >= 6) THEN BEGIN
         IF ((TP_EVA=99) OR (TP_EVA2=99) OR (TP_EVA3=99) OR (TP_EVA3<6)) THEN BEGIN
                 IF (NOTADIC = 0) THEN
                    CONDICION = 'DICIEMBRE';
                 ELSE IF (NOTADIC = 90 AND NOTAMAR = 0) THEN
                      CONDICION = 'ENPROCESO';
                 ELSE BEGIN
                    IF ((NOTADIC IN (99,90)) OR (NOTADIC < 6)) THEN BEGIN
                        IF (NOTAMAR = 0) THEN
                          CONDICION = 'MARZO';
                        ELSE BEGIN
                              IF ((NOTAMAR = 99) OR (NOTAMAR < 6)) THEN
                                CONDICION = 'PREVIA';
                              ELSE BEGIN
                                    CONDICION = 'REGULAR';
                                    NOTAFIN=NOTAMAR;
                                    NOTAFIN_FECHA = FECHMAR;
                                    IF (FECHMAR IS NULL) THEN BEGIN
                                       FERRCOD=2;
                                       FERRMSG='Si carga nota de marzo cargue la fecha';
                                       SUSPEND;
                                       EXIT;
                                    END
                              END
                        END
                    END
                    ELSE BEGIN
                        CONDICION = 'REGULAR';
                        --NOTAFIN=(PROM+NOTADIC)/2;--NOTADIC
                        NOTAFIN=NOTADIC;
                        NOTAFIN_FECHA = fechdic;
                        IF (FECHDIC IS NULL) THEN BEGIN
                             FERRCOD=2;
                             FERRMSG='Si carga nota de diciembre cargue la fecha';
                             SUSPEND;
                             EXIT;
                        END
                    END
                 END
         END
         ELSE BEGIN
            CONDICION = 'REGULAR';
            NOTAFIN=PROM;
            NOTAFIN_FECHA = FEC_EVA3;
         END
    END
    ELSE BEGIN  --IF ((PROM >=4) AND (PROM < 6)) THEN  BEGIN
                   IF (NOTADIC = 0) THEN
                      CONDICION = 'DICIEMBRE';
                   ELSE IF (NOTADIC = 90 AND NOTAMAR = 0) THEN
                      CONDICION = 'ENPROCESO';
                   ELSE IF ((NOTADIC < 6 or NOTADIC IN (90,99)) AND NOTAMAR IN (90)) THEN
                      CONDICION = 'ENPROCESO';
                   ELSE BEGIN
                       IF ((NOTADIC IN (99,90)) OR (NOTADIC < 6)) THEN BEGIN
                            IF (NOTAMAR = 0) THEN
                              CONDICION = 'MARZO';
                            ELSE BEGIN
                                  IF ((NOTAMAR = 99) OR (NOTAMAR < 6)) THEN
                                    CONDICION = 'PREVIA';
                                  ELSE BEGIN
                                    CONDICION = 'REGULAR';
                                    NOTAFIN=NOTAMAR;
                                    NOTAFIN_FECHA = fechmar;
                                    IF (FECHMAR IS NULL) THEN BEGIN
                                       FERRCOD=2;
                                       FERRMSG='Si carga nota de marzo cargue la fecha';
                                       SUSPEND;
                                       EXIT;
                                    END
                                  END
                            END
                        END
                        ELSE BEGIN
                          CONDICION = 'REGULAR';
                          --NOTAFIN=(PROM+NOTADIC)/2;--NOTADIC;
                          NOTAFIN=NOTADIC;
                          NOTAFIN_FECHA = fechdic;
                          IF (FECHDIC IS NULL) THEN BEGIN
                              FERRCOD=2;
                              FERRMSG='Si carga nota de diciembre cargue la fecha';
                              SUSPEND;
                              EXIT;
                          END
                        END
                   END
    END
   /* ELSE IF (PROM < 4) THEN BEGIN
                 IF (NOTAMAR = 0) THEN
                      CONDICION = 'MARZO';
                 ELSE BEGIN
                       IF ((NOTAMAR = 99) OR (NOTAMAR < 6)) THEN
                             CONDICION = 'PREVIA';
                       ELSE BEGIN
                             CONDICION = 'REGULAR';
                             NOTAFIN=NOTAMAR;
                             IF (FECHMAR IS NULL) THEN BEGIN
                                  FERRCOD=2;
                                  FERRMSG='Si carga nota de marzo cargue la fecha';
                                  SUSPEND;
                                  EXIT;
                             END
                       END
                 END
          END*/
/*END
ELSE
   CONDICION=CONDIORG;*/
   
FERRCOD=1;
FERRMSG='La meteria queda en condicion de '||CONDICION;
IF (CONDICION='REGULAR') THEN
  FERRMSG=FERRMSG||', Nota final '||NOTAFIN;
UPDATE "$$$CURSADA" X SET X.CONDICION=:CONDICION, X.notafin=:notafin, X.notafin_fecha=:NOTAFIN_FECHA WHERE COD_ALU=:CODALU AND X.COD_MAT=:CODMAT AND X.USUARIO=:USUARIO;
SUSPEND;
end ^

SET TERM ; ^
