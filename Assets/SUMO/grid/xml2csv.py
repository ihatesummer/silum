# Importing the required libraries
import xml.etree.ElementTree as Xet
import pandas as pd
import sys

input_xml_name = sys.argv[-1]
print(f"Parsing {input_xml_name} ...")
xmlparse = Xet.parse(input_xml_name)
root = xmlparse.getroot()
rows = []
for timestep in root:
    time = float(timestep.attrib['time'])
    for vehicle in timestep:
        id = int(float(vehicle.attrib['id']))
        pos_x = float(vehicle.attrib['x'])
        pos_y = float(vehicle.attrib['y'])
        # navigational standard
        # (0-360 degrees, going clockwise with 0
        # at the 12'o clock position)
        angle = float(vehicle.attrib['angle']) # degrees
        speed = float(vehicle.attrib['speed']) # [m/s]
        rows.append({"time [s]": time,
                    "id": id,
                    "pos_x": pos_x,
                    "pos_y": pos_y,
                    "angle [deg]": angle,
                    "speed [m/s]": speed})
df = pd.DataFrame(rows)
output_csv_name = input_xml_name[:-4] + ".csv"
df.to_csv(output_csv_name, index=False)
print(f"Saved {output_csv_name}.")
