class_name VectorWizard

static var rng = RandomNumberGenerator.new()

static var opposite_directions_v2 = {
	Vector2.UP: Vector2.DOWN,
	Vector2.DOWN: Vector2.UP,
	Vector2.RIGHT: Vector2.LEFT,
	Vector2.LEFT: Vector2.RIGHT
}

static var opposite_directions_v3 = {
  	Vector3.UP: Vector3.DOWN,
	Vector3.DOWN: Vector3.UP,
	Vector3.RIGHT: Vector3.LEFT, 
	Vector3.LEFT: Vector3.RIGHT, 
	Vector3.FORWARD: Vector3.BACK, 
	Vector3.BACK: Vector3.FORWARD
}

static func up_direction_opposite_vector2(up_direction: Vector2) -> Vector2:
	if opposite_directions_v2.has(up_direction):
		return opposite_directions_v2[up_direction]
	
	return Vector2.ZERO


static func up_direction_opposite_vector3(up_direction: Vector3) -> Vector3:
	if opposite_directions_v3.has(up_direction):
		return opposite_directions_v3[up_direction]
	
	return Vector3.ZERO
	

static func generate_2d_random_directions_using_degrees(num_directions: int = 10, origin: Vector2 = Vector2.UP, min_angle: float = 0.0, max_angle: float = 360.0) -> Array[Vector2]:
	var random_directions: Array[Vector2] = []

	for direction in range(num_directions):
		random_directions.append(origin.rotated(generate_random_angle_in_degrees(min_angle, max_angle)))

	return random_directions


static func generate_2d_random_directions_using_radians(num_directions: int = 10, origin: Vector2 = Vector2.UP, min_angle: float = 0.0, max_angle: float = 6.2831853072) -> Array[Vector2]:
	var random_directions: Array[Vector2] = []
	
	for direction in range(num_directions):
		random_directions.append(origin.rotated(generate_random_angle_in_radians(min_angle, max_angle)))

	return random_directions


static func generate_3d_random_directions_using_degrees(num_directions: int = 10, origin: Vector3 = Vector3.UP, min_angle: float = 0.0, max_angle: float = 360.0) -> Array[Vector3]:
	var random_directions: Array[Vector3] = []
	
	for direction in range(num_directions):
		random_directions.append(origin.rotated(Vector3.UP, generate_random_angle_in_degrees(min_angle, max_angle)))

	return random_directions


static func generate_3d_random_directions_using_radians(num_directions: int = 10, origin: Vector3 = Vector3.UP, min_angle: float = 0.0, max_angle: float = 6.2831853072) -> Array[Vector3]:
	var random_directions: Array[Vector3] = []
	
	for direction in range(num_directions):
		random_directions.append(origin.rotated(Vector3.UP, generate_random_angle_in_radians(min_angle, max_angle)))

	return random_directions


static func generate_random_angle_in_radians(min_angle: float = 0.0, max_angle: float = 6.2831853072) -> float:
	return min_angle + randf() * (max_angle - min_angle)


static func generate_random_angle_in_degrees(min_angle: float = 0.0, max_angle: float = 360.0) -> float:
	return min_angle + randf() * (max_angle - min_angle)


static func generate_2d_random_direction() -> Vector2:
	return Vector2(rng.randf_range(-1, 1), rng.randf_range(-1, 1)).normalized()


static func generate_2d_random_fixed_direction() -> Vector2:
	return Vector2(rng.randi_range(-1, 1), rng.randi_range(-1, 1)).normalized()


static func generate_3d_random_direction() -> Vector3:
	return Vector3(rng.randf_range(-1, 1), rng.randf_range(-1, 1), rng.randf_range(-1, 1)).normalized()


static func generate_3d_random_fixed_direction() -> Vector3:
	return Vector3(rng.randi_range(-1, 1), rng.randi_range(-1, 1), rng.randi_range(-1, 1)).normalized()


static func generate_random_mesh_surface_position(target: MeshInstance3D) -> Vector3:
	if target.mesh:
		var target_mesh_faces = target.mesh.get_faces()
		var random_face: Vector3 = target_mesh_faces[rng.randi() % target_mesh_faces.size()]
		
		random_face = Vector3(abs(random_face.x), abs(random_face.y), abs(random_face.z))
		
		return Vector3(
			rng.randf_range(-random_face.x, random_face.x),
		 	rng.randf_range(-random_face.y, random_face.y), 
			rng.randf_range(-random_face.z, random_face.z)
		)
		
	return Vector3.ZERO


static func translate_x_axis_to_vector(axis: float) -> Vector2:
	var horizontal_direction = Vector2.ZERO
	
	match axis:
		-1.0:
			horizontal_direction = Vector2.LEFT 
		1.0:
			horizontal_direction = Vector2.RIGHT
			
	return horizontal_direction


static func translate_y_axis_to_vector(axis: float) -> Vector2:
	var vertical_direction = Vector2.ZERO
	
	match axis:
		-1.0:
			vertical_direction = Vector2.UP 
		1.0:
			vertical_direction = Vector2.DOWN
			
	return vertical_direction


static func normalize_vector2(value: Vector2) -> Vector2:
		var direction := normalize_diagonal_vector2(value)
		
		if direction.is_equal_approx(value):
			return value if value.is_normalized() else value.normalized()
		
		return direction


static func normalize_diagonal_vector2(direction: Vector2) -> Vector2:
	if is_diagonal_direction_v2(direction):
		return direction * sqrt(2)
	
	return direction


static func is_diagonal_direction_v2(direction: Vector2) -> bool:
	return direction.x != 0 and direction.y != 0
	

static func normalize_vector3(value: Vector3) -> Vector3:
		var direction := normalize_diagonal_vector3(value)
		
		if direction.is_equal_approx(value):
			return value if value.is_normalized() else value.normalized()
		
		return direction


static func normalize_diagonal_vector3(direction: Vector3) -> Vector3:
	if is_diagonal_direction_v3(direction):
		return direction * sqrt(3)
	
	return direction


static func is_diagonal_direction_v3(direction: Vector3) -> bool:
	return direction.x != 0 and direction.y != 0 and direction.z != 0
	

## Also known as the "city distance" or "L1 distance".
## It measures the distance between two points as the sum of the absolute differences of their coordinates in each dimension.
## 
static func distance_manhattan_v2(a: Vector2, b: Vector2) -> float:
	return abs(a.x - b.x) + abs(a.y - b.y)

## Also known as the "chess distance" or "L∞ distance".
## It measures the distance between two points as the greater of the absolute differences of their coordinates in each dimension
static func distance_chebyshev_v2(a: Vector2, b: Vector2) -> float:
	return max(abs(a.x - b.x), abs(a.y - b.y))


static func length_manhattan_v2(a : Vector2) -> float:
	return abs(a.x) + abs(a.y)


static func length_chebyshev_v2(a : Vector2) -> float:
	return max(abs(a.x), abs(a.y))

## This function calculates the closest point on a line segment (defined by two points, a and b) to a third point c. 
## It also clamps the result to ensure that the closest point lies within the line segment.
static func closest_point_on_line_clamped_v2(a: Vector2, b: Vector2, c: Vector2) -> Vector2:
	b = (b - a).normalized()
	c = c - a
	return a + b * clamp(c.dot(b), 0.0, 1.0)

## This function is similar to the previous one but does not clamp the result. 
## It calculates the closest point on the line segment defined by a and b to a third point c.
## It uses the same vector operations as the previous closest_point_on_line_clamped_v2 function.
static func closest_point_on_line_v2(a: Vector2, b: Vector2, c: Vector2) -> Vector2:
	b = (b - a).normalized()
	c = c - a
	
	return a + b * (c.dot(b))

## 
# Vector3 versions of the above calculation functions
##
static func distance_manhattan_v3(a: Vector3, b: Vector3) -> float:
	return abs(a.x - b.x) + abs(a.y - b.y) + abs(a.z - b.z)


static func distance_chebyshev_v3(a: Vector3, b: Vector3) -> float:
	return max(abs(a.x - b.x), max(abs(a.y - b.y), abs(a.z - b.z)))


static func length_manhattan_v3(a: Vector3) -> float:
	return abs(a.x) + abs(a.y) + abs(a.z)


static func length_chebyshev_v3(a: Vector3) -> float:
	return max(abs(a.x), max(abs(a.y), abs(a.z)))


static func closest_point_on_line_v3(a: Vector3, b: Vector3, c: Vector3) -> Vector3:
	b = (b - a).normalized()
	c = c - a
	return a + b * (c.dot(b))


static func closest_point_on_line_clamped_v3(a: Vector3, b: Vector3, c: Vector3) -> Vector3:
	b = (b - a).normalized()
	c = c - a
	return a + b * clamp(c.dot(b), 0.0, 1.0)


static func closest_point_on_line_normalized_v3(a: Vector3, b: Vector3, c: Vector3) -> float:
	b = b - a
	c = c - a
	return c.dot(b.normalized()) / b.length()


static func rotate_horizontal_random(origin: Vector3 = Vector3.ONE) -> Vector3:
	var arc_direction: Vector3 = [Vector3.DOWN, Vector3.UP].pick_random()
	
	return origin.rotated(arc_direction, rng.randf_range(-PI / 2, PI / 2))


static func rotate_vertical_random(origin: Vector3 = Vector3.ONE) -> Vector3:
	var arc_direction: Vector3 = [Vector3.RIGHT, Vector3.LEFT].pick_random()
	
	return origin.rotated(arc_direction, rng.randf_range(-PI / 2, PI / 2))


static func flip_v2(vector: Vector2) -> Vector2:
	return -vector


static func flip_v3(vector: Vector3) -> Vector3:
	return -vector


static func is_withing_distance_squared_v2(vector: Vector2, second_vector: Vector2, distance: float) -> bool:
	return vector.distance_squared_to(second_vector) <= distance * distance
	

static func is_withing_distance_squared_v3(vector: Vector3, second_vector: Vector3, distance: float) -> bool:
	return vector.distance_squared_to(second_vector) <= distance * distance
	

static func direction_from_rotation_v2(rotation: float) -> Vector2:
	return Vector2(cos(rotation * PI / PI), sin(rotation * PI / PI))


static func direction_from_rotation_v3(rotation: float) -> Vector3:
	return Vector3(cos(rotation * PI / PI), sin(rotation * PI / PI), 0)


static func direction_from_rotation_degrees_v2(rotation_degrees: float) -> Vector2:
	rotation_degrees = deg_to_rad(rotation_degrees)
	var pi_degrees := deg_to_rad(PI)
	
	return Vector2(cos(rotation_degrees * pi_degrees / pi_degrees), sin(rotation_degrees * pi_degrees / pi_degrees))


static func direction_from_rotation_degrees_v3(rotation_degrees: float) -> Vector3:
	rotation_degrees = deg_to_rad(rotation_degrees)
	var pi_degrees := deg_to_rad(PI)
	
	return Vector3(cos(rotation_degrees * PI / pi_degrees), sin(rotation_degrees * PI / pi_degrees), 0)


static func random_inside_unit_circle(position: Vector2, radius: float = 1.0):
	var angle := randf() * 2.0 * PI
	return position + Vector2(cos(angle), sin(angle)) * radius



static func random_on_unit_circle(position: Vector2):
	var angle := randf() * 2.0 * PI
	var radius := randf()
	
	return position + radius * Vector2(cos(angle), sin(angle))
	

static func scale_vector2(vector: Vector2, length: float)-> Vector2:
	return vector.normalized() * length


static func clamp_vector2(vector: Vector2, min_length: float, max_length: float)->Vector2:
	return scale_vector2(vector, clamp(vector.length(), min_length, max_length))


static func scale_vector3(vector: Vector3, length: float)-> Vector3:
	return vector.normalized() * length


static func clamp_vector3(vector: Vector3, min_length: float, max_length: float)->Vector3:
	return scale_vector3(vector, clamp(vector.length(), min_length, max_length))
