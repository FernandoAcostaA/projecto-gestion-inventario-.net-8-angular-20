import os
from datetime import datetime

# PROBLEMA 1: Hardcoded Secret (Seguridad)
API_KEY = "sk_live_51234567890abcdef" 
DB_PASSWORD = "admin123"

class AnalizadorVentas:
    def __init__(self):
        # PROBLEMA 2: Unused Private Field (Code Smell)
        self._cache_temporal = [] 
        self.umbral_alerta = 1000

    def calcular_comision(self, monto, tipo_vendedor):
        """Calcula comisión con lógica anidada excesiva."""
        comision = 0
        
        # PROBLEMA 3: High Cyclomatic Complexity (Maintainability)
        if tipo_vendedor == "Senior":
            if monto > 5000:
                if datetime.now().month == 12:
                    comision = monto * 0.15
                else:
                    comision = monto * 0.10
            elif monto > 2000:
                comision = monto * 0.08
            else:
                comision = monto * 0.05
        elif tipo_vendedor == "Junior":
            if monto > 3000:
                comision = monto * 0.05
            else:
                comision = monto * 0.03
        else:
            comision = monto * 0.01
            
        # PROBLEMA 4: Magic Numbers (Code Smell)
        if comision > 500: 
            return round(comision * 0.9, 2)
        return comision

    def obtener_ruta_exportacion(self):
        # Uso de variable no usada y string hardcodeado
        ruta_base = "/tmp/exports"
        fecha = datetime.now().strftime("%Y%m%d")
        return f"{ruta_base}/reporte_{fecha}.csv"