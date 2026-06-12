SET TERM ^ ;

ALTER PROCEDURE XXX_REGULARIZACION (CARRE VARCHAR(6) CHARACTER SET NONE,
USUARIO SMALLINT)
RETURNS (FERRCOD INTEGER,
FERRMSG VARCHAR(100) CHARACTER SET NONE)
AS 
declare variable cod_alu char(11);
declare variable cutuco smallint;
declare variable cod_mat char(2);
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
declare variable nom_ape varchar(40);
declare variable tp_eva3 numeric(5,2);
declare variable prom numeric(5,2);
declare variable instituto varchar(30);
declare variable caracte varchar(6);
declare variable matriz varchar(5);
declare variable fec_eva1 date;
declare variable fec_eva2 date;
declare variable fec_eva3 date;
declare variable fal_eva1 numeric(5,2);
declare variable fal_eva2 numeric(5,2);
declare variable fal_eva3 numeric(5,2);
declare variable fecha date;
declare variable cond_ant varchar(20);
declare variable notadic numeric(5,2);
declare variable notamar numeric(5,2);
declare variable fechmar date;
declare variable fechdic date;
declare variable tipo varchar(3);
declare variable promocion char(1);
declare variable notaana numeric(5,2);
declare variable fechaana date;
declare variable notafin numeric(5,2);
declare variable notafin_fecha date;
declare variable aprsfinal varchar(1);
begin
    FERRCOD=0;
    FERRMSG='';
    SELECT TIPO, "INSTITUT", CARACT FROM CARRERA WHERE CARRE=:CARRE INTO :TIPO, :INSTITUTO,:CARACTE;

    FOR SELECT X.COD_ALU,X.CUTUCO,X.COD_MAT,X.CUA_ANIO,X.CONDICION,X.TP_EVA,X.RECUP,X.TP_EVA2,X.RECUP2,X.REGULAR,X.TOT_HORAS,X.INASIST,
               X.JUSTIF,X.FINAL1,X.FECHA1,X.FINAL2,X.FECHA2,X.APELLIDO,X.MATRIZ,X.USUARIO,X.NOM_APE,X.TP_EVA3,X.PROM, X.FEC_EVA1,
               X.FEC_EVA2,X.FEC_EVA3,X.FAL_EVA1,X.FAL_EVA2,X.FAL_EVA3, X.NOTADIC, X.NOTAMAR, X.FECHMAR, X.FECHDIC, M.PROMOCION,
               X.notafin, X.notafin_fecha, M.aprsfinal
    FROM "$$$CURSADA" X
    LEFT OUTER JOIN MATERIAS M ON M.CODMATERI=X.COD_MAT AND M.CODCARRE=:CARRE
    WHERE X.USUARIO = :USUARIO AND COALESCE(X.CONDICION,'')<>''
    INTO  :COD_ALU,:CUTUCO,:COD_MAT,:CUA_ANIO,:CONDICION,:TP_EVA,:RECUP,:TP_EVA2,:RECUP2,:REGULAR,
          :TOT_HORAS,:INASIST,:JUSTIF,:FINAL1,:FECHA1,:FINAL2,:FECHA2,:APELLIDO,:MATRIZ,:USUARIO,:NOM_APE,
          :TP_EVA3,:PROM, :FEC_EVA1,:FEC_EVA2,:FEC_EVA3,:FAL_EVA1,:FAL_EVA2,:FAL_EVA3, :NOTADIC, :NOTAMAR,
          :FECHMAR, :FECHDIC, :PROMOCION, :NOTAFIN, :NOTAFIN_FECHA, :aprsfinal
    DO BEGIN
         IF (TIPO='TER') THEN BEGIN
             UPDATE CURSADA SET TP_EVA=:TP_EVA, TP_EVA2=:TP_EVA2,RECUP=:RECUP,CONDICION=:CONDICION,
                                TOT_HORAS=:TOT_HORAS, INASIST=:INASIST, JUSTIF =:JUSTIF, USUARIO= :USUARIO
             WHERE CARRE =:CARRE AND COD_ALU=:COD_ALU AND COD_MAT=:COD_MAT AND CUA_ANIO=:CUA_ANIO;

             IF ((COALESCE(PROMOCION,'N')='S' AND CONDICION='PROMOCIONA') OR
                  (COALESCE(aprsfinal,'N')='S' AND CONDICION='FINAL')) THEN BEGIN

                IF ((COALESCE(PROMOCION,'N')='S')) THEN
                   NOTAANA=(TP_EVA+TP_EVA2)/2;
                ELSE if (COALESCE(aprsfinal,'N')='S') then BEGIN
                    if ((TP_EVA = 99 or TP_EVA2 = 99) or (TP_EVA < 4 or TP_EVA2 < 4)) then BEGIN
                        NOTAANA= :recup;
                    END
                    ELSE
                        NOTAANA=(TP_EVA+TP_EVA2)/2;    
                END

                SELECT IIF(SUBSTRING(:CUA_ANIO FROM 1 FOR 1)='1',T.FHTAPRI,T.FHTASEG)
                FROM TBL_CUAT T
                WHERE T.FANIO='20'||SUBSTRING(:CUA_ANIO FROM 2 FOR 2)
                INTO :FECHAANA;
                if (fechaana IS NULL) then BEGIN
                    EXCEPTION E_CUSTOM_ERR 'Falta fecha de promocion. Revise la tabla de fechas de cuatrimestre para el ano '||SUBSTRING(:CUA_ANIO FROM 2 FOR 2);
                END
                SELECT A.MATRIZ FROM ALUMNOS A WHERE A.COD_ALU=:COD_ALU AND A.CARRE=:CARRE INTO :MATRIZ;
                INSERT INTO CURSADA_HST (COD_ALU, APELLIDO, CARRE, CUTUCO, COD_MAT, CUA_ANIO, TP_EVA, RECUP, TP_EVA2, RECUP2,
                                         REGULAR, TOT_HORAS, INASIST, JUSTIF, CONDICION, FINAL1, FECHA1, FINAL2, FECHA2,
                                         FINAL3, FECHA3, FINAL4, FECHA4, MATRIZ, INSTITUT, CARAC, ACTINT, ACTDGE, ACTSNE,
                                         NREG, COLEGIO, "PLAN", A_C, DEFINE, INDICE, TP_EVA3, PROM, USUARIO, FEC_EVA1,
                                         FEC_EVA2, FEC_EVA3, FAL_EVA1, FAL_EVA2, FAL_EVA3, CONDANT, FACTFIN1,FACTFIN2,FACTFIN3)
                                  SELECT COD_ALU, APELLIDO, CARRE, CUTUCO, COD_MAT, CUA_ANIO, TP_EVA, RECUP, TP_EVA2, RECUP2,
                                         REGULAR, TOT_HORAS, INASIST, JUSTIF, CONDICION, FINAL1, FECHA1, FINAL2, FECHA2,
                                         FINAL3, FECHA3, FINAL4, FECHA4, MATRIZ, INSTITUT, CARAC, ACTINT, ACTDGE, ACTSNE,
                                         NREG, COLEGIO, "PLAN", A_C, DEFINE, INDICE, TP_EVA3, PROM, USUARIO, FEC_EVA1,
                                         FEC_EVA2, FEC_EVA3, FAL_EVA1, FAL_EVA2, FAL_EVA3, :COND_ANT, FACTFIN1,FACTFIN2,FACTFIN3
                                  FROM CURSADA C
                                  WHERE C.CARRE=:CARRE AND C.COD_ALU=:COD_ALU AND C.COD_MAT=:COD_MAT;
                  DELETE FROM CURSADA C  WHERE C.COD_ALU=:COD_ALU AND C.CARRE=:CARRE AND C.COD_MAT=:COD_MAT;
                  INSERT INTO ANALITIC(COD_ALU,APELLIDO,COD_MAT,CUA_ANIO,NOTA_MAT,
                                     FEC_FINAL,MATRIZ,CONDICION,"INSTITUT",CARAC,ACTINT,ACTDGE,ACTSNE,
                                     CARRE,COLEGIO,"PLAN",A_C,NREG,USUARIO,FACTFIN)
                              VALUES(:COD_ALU, :APELLIDO,:COD_MAT,:CUA_ANIO,:NOTAANA,:FECHAANA,:MATRIZ,
                                     'FINAL',:INSTITUTO,:CARACTE,NULL,NULL,NULL,:CARRE,NULL,NULL,NULL,NULL,:USUARIO,NULL); 

             END
         END
         ELSE IF (TIPO IN ('BAC','BAD') AND CARRE NOT IN ('333','650')) THEN BEGIN
             COND_ANT=NULL;
             SELECT CONDICION
             FROM CURSADA
             WHERE CARRE =:CARRE AND COD_ALU=:COD_ALU AND COD_MAT=:COD_MAT AND CUA_ANIO=:CUA_ANIO
             INTO :COND_ANT;
             UPDATE CURSADA SET TP_EVA=:TP_EVA, TP_EVA2=:TP_EVA2,RECUP=:RECUP,CONDICION=:CONDICION,
                                TOT_HORAS=:TOT_HORAS, INASIST=:INASIST, JUSTIF =:JUSTIF,
                                REGULAR=:REGULAR, FECHA1=:FECHA1,FINAL1=:FINAL1,USUARIO= :USUARIO
             WHERE CARRE =:CARRE AND COD_ALU=:COD_ALU AND COD_MAT=:COD_MAT AND CUA_ANIO=:CUA_ANIO;
             IF (TRIM(CONDICION)='REGULAR') THEN BEGIN
                SELECT A.MATRIZ FROM ALUMNOS A WHERE A.COD_ALU=:COD_ALU AND A.CARRE=:CARRE INTO :MATRIZ;
                INSERT INTO CURSADA_HST (COD_ALU, APELLIDO, CARRE, CUTUCO, COD_MAT, CUA_ANIO, TP_EVA, RECUP, TP_EVA2, RECUP2,
                                         REGULAR, TOT_HORAS, INASIST, JUSTIF, CONDICION, FINAL1, FECHA1, FINAL2, FECHA2,
                                         FINAL3, FECHA3, FINAL4, FECHA4, MATRIZ, INSTITUT, CARAC, ACTINT, ACTDGE, ACTSNE,
                                         NREG, COLEGIO, "PLAN", A_C, DEFINE, INDICE, TP_EVA3, PROM, USUARIO, FEC_EVA1,
                                         FEC_EVA2, FEC_EVA3, FAL_EVA1, FAL_EVA2, FAL_EVA3, CONDANT)
                                  SELECT COD_ALU, APELLIDO, CARRE, CUTUCO, COD_MAT, CUA_ANIO, TP_EVA, RECUP, TP_EVA2, RECUP2,
                                         REGULAR, TOT_HORAS, INASIST, JUSTIF, CONDICION, FINAL1, FECHA1, FINAL2, FECHA2,
                                         FINAL3, FECHA3, FINAL4, FECHA4, MATRIZ, INSTITUT, CARAC, ACTINT, ACTDGE, ACTSNE,
                                         NREG, COLEGIO, "PLAN", A_C, DEFINE, INDICE, TP_EVA3, PROM, USUARIO, FEC_EVA1,
                                         FEC_EVA2, FEC_EVA3, FAL_EVA1, FAL_EVA2, FAL_EVA3, :COND_ANT
                                  FROM CURSADA C
                                  WHERE C.CARRE=:CARRE AND C.COD_ALU=:COD_ALU AND C.COD_MAT=:COD_MAT;
                DELETE FROM CURSADA C WHERE C.CARRE=:CARRE AND C.COD_ALU=:COD_ALU AND C.COD_MAT=:COD_MAT;
                INSERT INTO ANALITIC(COD_ALU,APELLIDO,COD_MAT,CUA_ANIO,NOTA_MAT,
                                     FEC_FINAL,MATRIZ,CONDICION,"INSTITUT",CARAC,ACTINT,ACTDGE,ACTSNE,
                                     CARRE,COLEGIO,"PLAN",A_C,NREG,USUARIO)
                              VALUES(:COD_ALU, :APELLIDO,:COD_MAT,:CUA_ANIO,:FINAL1,:FECHA1,:MATRIZ,
                                     :CONDICION,:INSTITUTO,:CARACTE,NULL,NULL,NULL,:CARRE,NULL,
                                     NULL,NULL,NULL,:USUARIO);
             END
         END
         ELSE IF (TIPO IN ('BAC','BAD') AND CARRE IN ('333','650')) THEN BEGIN

             COND_ANT=NULL;
             SELECT CONDICION
             FROM CURSADA
             WHERE CARRE =:CARRE AND COD_ALU=:COD_ALU AND COD_MAT=:COD_MAT AND CUA_ANIO=:CUA_ANIO
             INTO :COND_ANT;
             UPDATE CURSADA SET TP_EVA=:TP_EVA, TP_EVA2=:TP_EVA2, TP_EVA3=:TP_EVA3,RECUP=:RECUP,CONDICION=:CONDICION,
                                TOT_HORAS=:TOT_HORAS, INASIST=:INASIST, JUSTIF =:JUSTIF,
                                REGULAR=:REGULAR, FECHA1=:FECHA1,FINAL1=:FINAL1, PROM=:PROM, FINAL2=:FINAL2, USUARIO= :USUARIO,
                                FEC_EVA1=:FEC_EVA1,FEC_EVA2=:FEC_EVA2,FEC_EVA3=:FEC_EVA3,FAL_EVA1=:FAL_EVA1,FAL_EVA2=:FAL_EVA2,FAL_EVA3=:FAL_EVA3,
                                NOTADIC=:NOTADIC, NOTAMAR=:NOTAMAR, FECHDIC=:FECHDIC, FECHMAR=:FECHMAR
             WHERE CARRE =:CARRE AND COD_ALU=:COD_ALU AND COD_MAT=:COD_MAT AND CUA_ANIO=:CUA_ANIO;
             IF (TRIM(CONDICION)='REGULAR') THEN BEGIN

                SELECT A.MATRIZ
                FROM ALUMNOS A
                WHERE A.COD_ALU=:COD_ALU AND A.CARRE=:CARRE INTO :MATRIZ;
                SELECT "INSTITUT", CARACT FROM CARRERA WHERE CARRE=:CARRE INTO :INSTITUTO,:CARACTE;
                INSERT INTO CURSADA_HST (COD_ALU, APELLIDO, CARRE, CUTUCO, COD_MAT, CUA_ANIO, TP_EVA, RECUP, TP_EVA2, RECUP2,
                                         REGULAR, TOT_HORAS, INASIST, JUSTIF, CONDICION, FINAL1, FECHA1, FINAL2, FECHA2,
                                         FINAL3, FECHA3, FINAL4, FECHA4, MATRIZ, INSTITUT, CARAC, ACTINT, ACTDGE, ACTSNE,
                                         NREG, COLEGIO, "PLAN", A_C, DEFINE, INDICE, TP_EVA3, PROM, USUARIO, FEC_EVA1,
                                         FEC_EVA2, FEC_EVA3, FAL_EVA1, FAL_EVA2, FAL_EVA3, CONDANT, NOTADIC, NOTAMAR, FECHDIC,  FECHMAR, NOTAFIN, NOTAFIN_FECHA)
                                  SELECT COD_ALU, APELLIDO, CARRE, CUTUCO, COD_MAT, CUA_ANIO, TP_EVA, RECUP, TP_EVA2, RECUP2,
                                         REGULAR, TOT_HORAS, INASIST, JUSTIF, CONDICION, FINAL1, FECHA1, FINAL2, FECHA2,
                                         FINAL3, FECHA3, FINAL4, FECHA4, MATRIZ, INSTITUT, CARAC, ACTINT, ACTDGE, ACTSNE,
                                         NREG, COLEGIO, "PLAN", A_C, DEFINE, INDICE, TP_EVA3, PROM, USUARIO, FEC_EVA1,
                                         FEC_EVA2, FEC_EVA3, FAL_EVA1, FAL_EVA2, FAL_EVA3, :COND_ANT, NOTADIC, NOTAMAR, FECHDIC,
                                         FECHMAR, :notafin, :notafin_fecha
                                  FROM CURSADA C
                                  WHERE C.CARRE=:CARRE AND C.COD_ALU=:COD_ALU AND C.COD_MAT=:COD_MAT;
                DELETE FROM CURSADA C WHERE C.CARRE=:CARRE AND C.COD_ALU=:COD_ALU AND C.COD_MAT=:COD_MAT;

                INSERT INTO ANALITIC(COD_ALU,APELLIDO,COD_MAT,CUA_ANIO,NOTA_MAT,
                                     FEC_FINAL,MATRIZ,CONDICION,"INSTITUT",CARAC,ACTINT,ACTDGE,ACTSNE,
                                     CARRE,COLEGIO,"PLAN",A_C,NREG,USUARIO)
                              VALUES(:COD_ALU, :APELLIDO,:COD_MAT,:CUA_ANIO,:NOTAFIN,:NOTAFIN_FECHA,:MATRIZ,
                                     :CONDICION,:INSTITUTO,:CARACTE,NULL,NULL,NULL,:CARRE,NULL,
                                     NULL,NULL,NULL,:USUARIO);
             END

         END
  END

DELETE FROM "$$$CURSADA" WHERE USUARIO = :USUARIO;
SUSPEND;
end ^

SET TERM ; ^
