SET TERM ^ ;

ALTER PROCEDURE XXX_REGULARIZACION_MAT_POSTVAL (CARRE VARCHAR(6) CHARACTER SET NONE,
CODALU VARCHAR(11) CHARACTER SET NONE,
CODMAT VARCHAR(2) CHARACTER SET NONE,
USUARIO INTEGER,
PASO VARCHAR(20) CHARACTER SET NONE)
RETURNS (FERRCOD INTEGER,
FERRMSG VARCHAR(3000) CHARACTER SET NONE,
FBUTTONS VARCHAR(50) CHARACTER SET NONE)
AS 
declare variable cutuco smallint;
declare variable cua_anio char(3);
declare variable condicion char(15);
declare variable tp_eva numeric(5,2);
declare variable recup numeric(5,2);
declare variable tp_eva2 numeric(5,2);
declare variable recup2 numeric(5,2);
declare variable regular numeric(5,2);
declare variable tot_horas smallint;
declare variable inasist smallint;
declare variable justif smallint;
declare variable final1 numeric(5,2);
declare variable fecha1 date;
declare variable final2 numeric(5,2);
declare variable fecha2 date;
declare variable apellido char(25);
declare variable matriz char(5);
declare variable nom_ape varchar(40);
declare variable tp_eva3 numeric(5,2);
declare variable prom numeric(5,2);
declare variable fec_eva1 date;
declare variable fec_eva2 date;
declare variable fec_eva3 date;
declare variable fal_eva1 numeric(5,2);
declare variable fal_eva2 numeric(5,2);
declare variable fal_eva3 numeric(5,2);
declare variable notadic numeric(5,2);
declare variable notamar numeric(5,2);
declare variable fechdic date;
declare variable fechmar date;
declare variable resulj numeric(5,2);
declare variable resuli numeric(5,2);
declare variable resul numeric(6,2);
declare variable continasbac varchar(20);
declare variable condiorg varchar(20);
declare variable mtot_horas integer;
declare variable minasist integer;
declare variable promsave numeric(5,2);
begin

IF (CARRE<> 'BAC') THEN
    EXIT;


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

TP_EVA=COALESCE(TP_EVA,0);
TP_EVA2=COALESCE(TP_EVA2,0);
TP_EVA3=COALESCE(TP_EVA3,0);  --PROMEDIO DEL CUATRIMESTRE TPEVA/TPEVA2
REGULAR=COALESCE(REGULAR,0); --NOTA DE A/REGULAR
FINAL1=COALESCE(FINAL1,0);
RECUP=COALESCE(RECUP,0); --
--IF (CONDICION='LIBRES') THEN NO HAGO NADA DEJO TODO COMO ESTA
--condicion = cursando libres consejo
--exception e_custom_err condicion;
IF (CONDICION IN ('CURSANDO','RECURSANDO') AND PASO='') THEN BEGIN
    CONDIORG=CONDICION; --'CURSANDO';
    ---------------------
    CONTINASBAC=CONDICION; --'CURSANDO';
    RESUL=0;
    MTOT_HORAS=COALESCE(TOT_HORAS,0);
    MINASIST=COALESCE(INASIST,0);
    IF (MTOT_HORAS > 0) THEN
        RESUL= ROUND((MINASIST * 100)/ MTOT_HORAS);
    IF (MTOT_HORAS = 0) THEN
        CONTINASBAC = CONDICION;--'CURSANDO';
    ELSE BEGIN
      IF (RESUL>=0 AND RESUL <= 25)  THEN
          CONTINASBAC = 'REGULAR';
      IF (RESUL >= 26 AND RESUL <= 40) THEN
          CONTINASBAC = 'CONSEJO';
      IF (RESUL > 40) THEN
          CONTINASBAC = 'LIBRE';
    END
    IF (CONTINASBAC IN ('CONSEJO','LIBRE')) THEN BEGIN
        FERRCOD=2;
        FERRMSG='Error de postvalida, a este punto no puede llegar en condicion distinto de cursando '||CONTINASBAC;
        SUSPEND;
        EXIT;
    END
    --------***********
    IF (TP_EVA = 99 AND TP_EVA2 = 99) THEN BEGIN
        CONDICION = 'LIBRES';
    END
    ELSE IF (TP_EVA = 0 OR TP_EVA2 = 0)THEN BEGIN
        CONDICION = CONDIORG; --'CURSANDO';
    END
    ELSE IF (TP_EVA BETWEEN 6 AND 10 AND  TP_EVA2 BETWEEN 6 AND 10) THEN BEGIN
        IF (CONTINASBAC = 'CURSANDO') THEN
            CONDICION = CONDIORG;
        IF (CONTINASBAC = 'REGULAR') THEN BEGIN
            CONDICION='REGULAR';
            PROMSAVE = TP_EVA3;
        END
    END
    ELSE IF (TP_EVA2 BETWEEN 4 AND 10 AND TP_EVA3 >= 6) THEN BEGIN
        IF (CONTINASBAC = 'CURSANDO') THEN
            CONDICION = CONDIORG;
        IF (CONTINASBAC = 'REGULAR') THEN BEGIN
            CONDICION='REGULAR';
            PROMSAVE = TP_EVA3;
        END
    END
    ELSE IF (TP_EVA3 BETWEEN 1 AND 3.99) THEN BEGIN   --//
        IF (RECUP BETWEEN 6 AND 10) THEN  BEGIN
           IF (CONTINASBAC = 'CURSANDO') THEN
              CONDICION = CONDIORG;
           IF (CONTINASBAC = 'REGULAR') THEN BEGIN
              CONDICION='REGULAR';
              PROMSAVE = FINAL1;
           END
        END
        ELSE IF (REGULAR = 0 ) THEN BEGIN
              IF (CONTINASBAC = 'CURSANDO') THEN
                  CONDICION = CONDIORG;
              IF (CONTINASBAC = 'REGULAR') THEN
                  CONDICION = 'A/REGULAR';
        END
        ELSE IF (REGULAR BETWEEN 6 AND 10) THEN BEGIN
              IF (CONTINASBAC = 'CURSANDO') THEN
                  CONDICION = CONDIORG;
              IF (CONTINASBAC = 'REGULAR') THEN BEGIN
                  CONDICION='REGULAR';
                  PROMSAVE = REGULAR;
              END
        END
        ELSE IF (REGULAR BETWEEN 1 AND 5.99 OR REGULAR = 99) THEN BEGIN
               IF (CONTINASBAC = 'CURSANDO') THEN
                  CONDICION = CONDIORG;
               IF (CONTINASBAC = 'REGULAR') THEN
                  CONDICION = 'PREVIO';
        END
    END
    ELSE IF (TP_EVA BETWEEN 6 AND 10 AND TP_EVA2 BETWEEN 1 AND 3.99) THEN BEGIN
        IF (RECUP = 0) THEN  BEGIN
            IF (CONTINASBAC = 'CURSANDO') THEN
               CONDICION = CONDIORG;
            ELSE IF (CONTINASBAC = 'REGULAR') THEN
               CONDICION = 'CURSANDO';
        END
        ELSE IF (RECUP BETWEEN 6 AND 10) THEN  BEGIN
           IF (CONTINASBAC = 'CURSANDO') THEN
              CONDICION = CONDIORG;
           IF (CONTINASBAC = 'REGULAR') THEN BEGIN
              CONDICION='REGULAR';
              PROMSAVE = FINAL1;
           END
        END
        IF (RECUP BETWEEN 0.1 AND 5.99 OR RECUP = 99) THEN BEGIN
           IF (REGULAR = 0) THEN  BEGIN
               IF (CONTINASBAC = 'CURSANDO') THEN
                  CONDICION = CONDIORG;
               IF (CONTINASBAC = 'REGULAR') THEN
                  CONDICION = 'A/REGULAR';
           END
           ELSE IF (REGULAR BETWEEN 6 AND 10) THEN BEGIN
                IF (CONTINASBAC = 'CURSANDO') THEN
                  CONDICION = CONDIORG;
                IF (CONTINASBAC = 'REGULAR') THEN BEGIN
                     CONDICION='REGULAR';
                     PROMSAVE = REGULAR;
                END
           END
           IF (REGULAR BETWEEN 0.1 AND 5.99 OR REGULAR = 99) THEN BEGIN
                IF (CONTINASBAC = 'CURSANDO') THEN
                  CONDICION = CONDIORG;
                IF (CONTINASBAC = 'REGULAR') THEN
                  CONDICION = 'PREVIO';
           END
        END
    END
    ELSE IF (TP_EVA3 >= 4 AND TP_EVA3 < 6) THEN BEGIN
        IF (RECUP = 0) THEN BEGIN
           IF (CONTINASBAC = 'CURSANDO') THEN
              CONDICION = CONDIORG;
           IF (CONTINASBAC = 'REGULAR') THEN
              CONDICION = CONDIORG; --'CURSANDO';
        END
        ELSE IF (RECUP BETWEEN 6 AND 10) THEN BEGIN
           IF (CONTINASBAC = 'CURSANDO') THEN
              CONDICION = CONDIORG;
           IF (CONTINASBAC = 'REGULAR') THEN BEGIN
                CONDICION='REGULAR';
                PROMSAVE = FINAL1;
           END
        END
        ELSE IF (RECUP BETWEEN 1 AND 5.99 OR RECUP = 99) THEN BEGIN
            IF (REGULAR = 0) THEN BEGIN
               IF (CONTINASBAC = 'CURSANDO') THEN
                  CONDICION = CONDIORG;
               IF (CONTINASBAC = 'REGULAR') THEN
                  CONDICION = 'A/REGULAR';
            END
            IF (REGULAR BETWEEN 6 AND 10) THEN BEGIN
                IF (CONTINASBAC = 'CURSANDO') THEN
                  CONDICION = CONDIORG;
                IF (CONTINASBAC = 'REGULAR') THEN BEGIN
                     CONDICION='REGULAR';
                     PROMSAVE = REGULAR;
                END
            END
            IF (REGULAR BETWEEN 1 AND 5.99 OR REGULAR = 99) THEN  BEGIN
                IF (CONTINASBAC = 'CURSANDO') THEN
                  CONDICION = CONDIORG;
                IF (CONTINASBAC = 'REGULAR') THEN
                  CONDICION = 'PREVIO';
            END
        END
    END
    FERRCOD=0;
    FERRMSG='El alumno quedo en condicion de '||CONDICION;
    IF (CONDICION='REGULAR') THEN
        FERRMSG=FERRMSG||', Nota final '||PROMSAVE;

    UPDATE "$$$CURSADA" X SET X.CONDICION=:CONDICION, FINAL1=:PROMSAVE WHERE COD_ALU=:CODALU AND X.COD_MAT=:CODMAT AND X.USUARIO=:USUARIO;
    SUSPEND;
END
ELSE IF (CONDICION='CONSEJO' AND PASO='') THEN BEGIN
    FERRCOD=3;
    FERRMSG='En que condicion quiere dejar al alumno?';
    FBUTTONS='Consejo,Regular,Libre';
    SUSPEND;
    EXIT;
END
ELSE IF (CONDICION='CONSEJO' AND PASO<>'') THEN BEGIN
    IF (PASO='Consejo') THEN BEGIN
        CONDICION='CONSEJO';
    END
    IF (PASO='Libre') THEN BEGIN
        CONDICION='LIBRES';
    END
    ELSE IF (PASO='Regular') THEN BEGIN
        CONTINASBAC='REGULAR';
        -----***********
        IF (TP_EVA = 99 AND TP_EVA2 = 99) THEN BEGIN
            CONDICION = 'LIBRES';
        END
        ELSE IF (TP_EVA = 0 OR TP_EVA2 = 0)THEN BEGIN
            CONDICION = 'CONSEJO';
        END
        ELSE IF (TP_EVA BETWEEN 6 AND 10 AND  TP_EVA2 BETWEEN 6 AND 10) THEN BEGIN
            IF (CONTINASBAC = 'REGULAR') THEN BEGIN
                CONDICION='REGULAR';
                PROMSAVE = TP_EVA3;
            END
        END
        ELSE IF (TP_EVA2 BETWEEN 4 AND 10 AND TP_EVA3 >= 6) THEN BEGIN
            IF (CONTINASBAC = 'REGULAR') THEN BEGIN
                CONDICION='REGULAR';
                PROMSAVE = TP_EVA3;
            END
        END
        ELSE IF (TP_EVA3 BETWEEN 1 AND 3.99) THEN BEGIN
            IF (RECUP BETWEEN 6 AND 10) THEN  BEGIN
                IF (CONTINASBAC = 'REGULAR') THEN BEGIN
                    CONDICION='REGULAR';
                    PROMSAVE = FINAL1;
                END
            END
            ELSE IF (REGULAR = 0 ) THEN BEGIN
                IF (CONTINASBAC = 'REGULAR') THEN
                    CONDICION = 'A/REGULAR';
            END
            ELSE IF (REGULAR BETWEEN 6 AND 10) THEN BEGIN
                IF (CONTINASBAC = 'REGULAR') THEN BEGIN
                    CONDICION='REGULAR';
                    PROMSAVE = REGULAR;
                END
            END
            ELSE IF (REGULAR BETWEEN 1 AND 5.99 OR REGULAR = 99) THEN BEGIN
                IF (CONTINASBAC = 'REGULAR') THEN
                    CONDICION = 'PREVIO';
            END
        END
        ELSE IF (TP_EVA BETWEEN 6 AND 10 AND TP_EVA2 BETWEEN 1 AND 3.99) THEN BEGIN
            IF (RECUP = 0) THEN  BEGIN
                IF (CONTINASBAC = 'REGULAR') THEN
                    CONDICION = 'CONSEJO';
            END
            ELSE IF (RECUP BETWEEN 6 AND 10) THEN  BEGIN
                IF (CONTINASBAC = 'REGULAR') THEN BEGIN
                    CONDICION='REGULAR';
                    PROMSAVE = FINAL1;
                END
            END
            IF (RECUP BETWEEN 0.1 AND 5.99 OR RECUP = 99) THEN BEGIN
                IF (REGULAR = 0) THEN  BEGIN
                    IF (CONTINASBAC = 'REGULAR') THEN
                        CONDICION = 'A/REGULAR';
                END
                ELSE IF (REGULAR BETWEEN 6 AND 10) THEN BEGIN
                    IF (CONTINASBAC = 'REGULAR') THEN BEGIN
                        CONDICION='REGULAR';
                        PROMSAVE = REGULAR;
                    END
                END
                IF (REGULAR BETWEEN 0.1 AND 5.99 OR REGULAR = 99) THEN BEGIN
                    IF (CONTINASBAC = 'REGULAR') THEN
                        CONDICION = 'PREVIO';
                END
            END
        END
        ELSE IF (TP_EVA3 >= 4 AND TP_EVA3 < 6) THEN BEGIN
            IF (RECUP = 0) THEN BEGIN
                IF (CONTINASBAC = 'REGULAR') THEN
                    CONDICION = 'CONSEJO';
            END
            ELSE IF (RECUP BETWEEN 6 AND 10) THEN BEGIN
                IF (CONTINASBAC = 'REGULAR') THEN BEGIN
                    CONDICION='REGULAR';
                    PROMSAVE = FINAL1;
                END
            END
            ELSE IF (RECUP BETWEEN 1 AND 5.99 OR RECUP = 99) THEN BEGIN
                IF (REGULAR = 0) THEN BEGIN
                    IF (CONTINASBAC = 'REGULAR') THEN
                        CONDICION = 'A/REGULAR';
                END
                IF (REGULAR BETWEEN 6 AND 10) THEN BEGIN
                    IF (CONTINASBAC = 'REGULAR') THEN BEGIN
                        CONDICION='REGULAR';
                        PROMSAVE = REGULAR;
                    END
                END IF (REGULAR BETWEEN 1 AND 5.99 OR REGULAR = 99) THEN  BEGIN
                    IF (CONTINASBAC = 'REGULAR') THEN
                        CONDICION = 'PREVIO';
                END
            END
        END
    END
    FERRCOD=0;
    FERRMSG='El alumno quedo en condicion de '||CONDICION;
    IF (CONDICION='REGULAR') THEN
        FERRMSG=FERRMSG||', Nota final '||PROMSAVE;

    UPDATE "$$$CURSADA" X SET X.CONDICION=:CONDICION, FINAL1=:PROMSAVE WHERE COD_ALU=:CODALU AND X.COD_MAT=:CODMAT AND X.USUARIO=:USUARIO;
    FBUTTONS='';
    SUSPEND;
    EXIT;
END
end ^

SET TERM ; ^
