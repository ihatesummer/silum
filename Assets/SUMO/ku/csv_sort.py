import pandas as pd

input_csv_name = "mobility_data_filtered_sorted_interpolated.csv"
output_csv_name = "mobility.csv"

print(f"Sorting {input_csv_name} ...")
df = pd.read_csv(input_csv_name, delimiter=',')
df = df.sort_values(["time [s]", "id"], ascending=[True, True])
df.to_csv(output_csv_name, index=False)
print(f"Saved {output_csv_name}.")
